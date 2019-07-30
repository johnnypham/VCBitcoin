using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using VCBitcoin.BlockchainParser;
using VCBitcoinBlockchainParser;

namespace VCBitcoin {

    public enum QueryType {
        Invalid,
        Height,
        InvalidHeight,
        Hash,
        BlockHash,
        TxId,
        Address,
        InvalidHash,
    }

    public class PostgreSQL {

        private NpgsqlConnectionStringBuilder builder;

        public PostgreSQL() {

            builder = new NpgsqlConnectionStringBuilder();
            builder.Host = "127.0.0.1";
            builder.Port = 5432;
            builder.Database = "VCBitcoin";
            builder.Username = "jp";
            builder.Password = "pw";
        }

        //public void Truncate() {
        //    using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
        //        conn.Open();
        //        using (var cmd = new NpgsqlCommand(string.Empty, conn)) {
        //            cmd.CommandText = "TRUNCATE Block, Transaction, Input, Output;";
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        public void Insert(Block block, string fileName, long blockOffset) {

            using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                conn.Open();
                using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable)) {
                    using (var cmd = new NpgsqlCommand(string.Empty, conn, transaction)) {

                        StringBuilder sb = new StringBuilder("INSERT INTO Block " +
                                                             "VALUES (@BlockHash,@FileName, " +
                                                             "@FileOffset,@Size," +
                                                             "@PreviousBlockHash,@Timestamp," +
                                                             "@TargetDifficulty,@nTransactions);");

                        cmd.Parameters.AddWithValue("BlockHash", block.blockHash.ToString());
                        cmd.Parameters.AddWithValue("FileName", fileName);
                        cmd.Parameters.AddWithValue("FileOffset", blockOffset);
                        cmd.Parameters.AddWithValue("Size", (long)block.size);
                        cmd.Parameters.AddWithValue("PreviousBlockHash", block.prevBlockHash.ToString());
                        cmd.Parameters.AddWithValue("Timestamp", block.creationTime);
                        cmd.Parameters.AddWithValue("TargetDifficulty", (long)block.targetDifficulty);
                        cmd.Parameters.AddWithValue("nTransactions", (long)block.nTxns);

                        InputTxn currInput;
                        OutputTxn currOutput;

                        int x = 0;
                        int y = 0;
                        int z = 0;

                        foreach (Transaction currTxn in block.txns) {

                            sb.Append("INSERT INTO Transaction " +
                                      "VALUES(@TransactionID" + x + "," +
                                      "@BlockHashTxn" + x + "," +
                                      "@IsSegwit" + x + ");");

                            cmd.Parameters.AddWithValue("TransactionID" + x, currTxn.txId.ToString());
                            cmd.Parameters.AddWithValue("BlockHashTxn" + x, block.blockHash.ToString());
                            cmd.Parameters.AddWithValue("IsSegwit" + x, currTxn.isSegwit);

                            for (int k = 0; k < currTxn.outputs.Length; ++k) {
                                currOutput = currTxn.outputs[k];

                                sb.Append("INSERT INTO Output " +
                                          "VALUES(@TransactionIDoutput" + y + "," +
                                          "@Index" + y + "," +
                                          "@AddressOutput" + y + "," +
                                          "@Value" + y + ");");

                                cmd.Parameters.AddWithValue("TransactionIDoutput" + y, currTxn.txId.ToString());
                                cmd.Parameters.AddWithValue("Index" + y, k);
                                cmd.Parameters.AddWithValue("AddressOutput" + y, currOutput.destinationAddress.ToString());
                                cmd.Parameters.AddWithValue("Value" + y, (double)currOutput.value / 100000000);
                                ++y;
                            }

                            for (int j = 0; j < currTxn.inputs.Length; ++j) {
                                currInput = currTxn.inputs[j];

                                if (currInput.vout != 0x11111111) {
                                    sb.Append("INSERT INTO Input " +
                                              "VALUES(@TransactionIDinput" + z + "," +
                                              "@Vout" + z + ");");
                                    cmd.Parameters.AddWithValue("TransactionIDinput" + z, currInput.txnHash.ToString());
                                    cmd.Parameters.AddWithValue("Vout" + z, (int)currInput.vout);
                                    ++z;
                                }
                            }

                            ++x;
                        }

                        cmd.CommandText = sb.ToString();
                        try {
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        } catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                            LogError(ex);
                        }
                    }
                }
            }
        }

        public void CreateIndexes() {
            using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                conn.Open();
                using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable)) {
                    using (var cmd = new NpgsqlCommand(string.Empty, conn, transaction)) {

                        cmd.CommandText = "CREATE INDEX INDEX_BLOCK_HASH ON Block(BlockHash);" +
                                          "CREATE INDEX INDEX_TIMESTAMP ON Block(Timestamp);" +
                                          "CREATE INDEX INDEX_N_TRANSACTIONS ON Block(nTransactions);" +
                                          "CREATE INDEX INDEX_TRANSACTION_ID ON Transaction(TransactionID);" +
                                          "CREATE INDEX INDEX_TXN_BLOCK_HASH ON Transaction(BlockHash);" +
                                          "CREATE INDEX INDEX_OUTPUT_TXID_ ON Output(TransactionID);" +
                                          "CREATE INDEX INDEX_OUTPUT_VOUT ON Output(Index);" +
                                          "CREATE INDEX INDEX_OUTPUT_ADDRESS ON Output(Address);" +
                                          "CREATE INDEX INDEX_OUTPUT_VALUE ON Output(Value);" +
                                          "CREATE INDEX INDEX_INPUT ON Input(TransactionID, Vout)";

                        try {
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        } catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                            LogError(ex);
                        }
                    }
                }
            }
        }

        public void RemoveStaleBlocks() {
            using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                conn.Open();
                using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable)) {
                    using (var cmd = new NpgsqlCommand(string.Empty, conn, transaction)) {

                        cmd.CommandText = "SELECT BlockHash, PreviousBlockHash FROM Block;";

                        NpgsqlDataReader reader = cmd.ExecuteReader();

                        List<Dictionary<string, string>> blockchain = new List<Dictionary<string, string>>();
                        Dictionary<string, string> block;

                        while (reader.Read()) {
                            block = new Dictionary<string, string>();
                            block.Add(reader[0].ToString(), reader[1].ToString());
                            blockchain.Add(block);
                        }

                        List<string> blocksToRemove = StaleBlockRemover.Clean(blockchain);
                        foreach (var s in blocksToRemove) {
                            Console.WriteLine(s);
                        }

                        StringBuilder sb = new StringBuilder("DELETE FROM Block " +
                                                             "WHERE BlockHash = @BlockHash0 ");
                        cmd.Parameters.AddWithValue("BlockHash0", blocksToRemove.ElementAt(0));

                        for (int i = 1; i < blocksToRemove.Count; ++i) {
                            sb.Append("OR BlockHash = @BlockHash " + i);
                            cmd.Parameters.AddWithValue("BlockHash" + i, blocksToRemove.ElementAt(i));
                        }

                        try {
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        } catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                            LogError(ex);
                        }
                    }
                }
            }
        }

        public void AddHeightColumn() {
            using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                conn.Open();
                using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable)) {
                    using (var cmd = new NpgsqlCommand(string.Empty, conn, transaction)) {

                        cmd.CommandText = "ALTER TABLE Block ADD COLUMN Height serial;" +
                                          "CREATE INDEX INDEX_HEIGHT ON Block(Height);";

                        try {
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        } catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                            LogError(ex);
                        }
                    }
                }
            }
        }

        public async Task<QueryType> CheckInput(string input) {

            Int64 height;

            if (Int64.TryParse(input, out height) && height >= 0) {
                using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(string.Empty, conn)) {
                        cmd.CommandText = "SELECT Height " +
                                          "FROM Block " +
                                          "ORDER BY Height DESC " +
                                          "FETCH FIRST 1 ROWS ONLY";

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read()) {
                            int tipHeight = (int)reader[0];
                            if (height <= tipHeight) {
                                return QueryType.Height;
                            } else {
                                return QueryType.InvalidHeight;
                            }
                        }
                    }
                }
            }

            if (input.Length >= 26 && input.Length <= 64) {
                return QueryType.Hash;
            }

            return QueryType.Invalid;
        }

        public async Task<string> SelectHash(string hash) {

            QueryType queryType;

            using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                conn.Open();
                using (var checkCmd = new NpgsqlCommand(string.Empty, conn)) {

                    checkCmd.CommandText = "SELECT 1 FROM BLOCK " +
                                      "WHERE BlockHash = @hash;" +
                                      "SELECT 1 FROM Transaction " +
                                      "WHERE TransactionId = @txid;" +
                                      "SELECT 1 FROM Output " +
                                      "WHERE Address = @address";

                    checkCmd.Parameters.AddWithValue("hash", hash);
                    checkCmd.Parameters.AddWithValue("txid", hash);
                    checkCmd.Parameters.AddWithValue("address", hash);
                    DbDataReader reader = await checkCmd.ExecuteReaderAsync();

                    if (reader.HasRows) {
                        queryType = QueryType.BlockHash;
                    } else {
                        reader.NextResult();
                        if (reader.HasRows) {
                            queryType = QueryType.TxId;
                        } else {
                            reader.NextResult();
                            queryType = QueryType.Address;
                        }
                    }
                }
            }

            if (queryType == QueryType.BlockHash) {
                using (var conn = new NpgsqlConnection(builder.ConnectionString)) {
                    conn.Open();
                    using (var checkCmd = new NpgsqlCommand(string.Empty, conn)) {
                        using (var cmd = new NpgsqlCommand(string.Empty, conn)) {

                            cmd.CommandText = "SELECT * FROM Block " +
                                          "WHERE BlockHash = @hash";

                            cmd.Parameters.AddWithValue("hash", hash);

                            StringBuilder sb = new StringBuilder();
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            while (reader.Read()) {
                                for (int i = 0; i < reader.FieldCount; ++i) {
                                    sb.Append(reader.GetName(i));
                                    sb.Append(": ");
                                    sb.Append(reader[i]);
                                    sb.Append("\n");
                                }
                            }
                            return sb.ToString();
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void LogError(Exception ex) {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string filePath = $"{desktop}\\errorlog.txt";

            using (StreamWriter writer = new StreamWriter(filePath, true)) {
                writer.WriteLine("--------------------------------------------------------");
                writer.WriteLine("Date: " + DateTime.Now.ToString());
                writer.WriteLine();
                writer.WriteLine(ex.GetType().FullName);
                writer.WriteLine("Message: " + ex.Message);
                writer.WriteLine("StackTrace: " + ex.StackTrace);
            }
        }

    }

}