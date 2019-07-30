using System;
using System.IO;
using VCBitcoin;

namespace VCBitcoinBlockchainParser {

    class BlockParser {

        public BlockParser() {

            Console.WriteLine($"start time {string.Format("{0:HH:mm:ss tt}", DateTime.Now) }");

            string blockchainPath = @"C:\Users\johnn\AppData\Roaming\Bitcoin\testnet3\blocks\";
            DirectoryInfo di = new DirectoryInfo(blockchainPath);
            FileInfo[] files = di.GetFiles("blk*.dat");

            Console.WriteLine($"{files.Length} files found");

            PostgreSQL postgreSql = new PostgreSQL();

            for (int i = 0; i < 10; ++i) {

                using (FileStream fs = new FileStream(files[i].FullName, FileMode.Open)) {
                    using (MemoryStream ms = new MemoryStream()) {
                        using (BinaryReader br = new BinaryReader(ms)) {

                            Console.WriteLine($"processing {files[i].Name} ({i}/{files.Length})");

                            double progress = 0;
                            int checkpoint = 0;
                            long blockOffset = 0;

                            UpdateProgress(progress, ref checkpoint);
                            fs.CopyTo(ms);
                            ms.Position = 0;

                            while (ms.Position < ms.Length) {

                                blockOffset = ms.Position;

                                Block block = new Block();
                                block.ReadStream(br);

                                postgreSql.Insert(block, files[i].Name, blockOffset);

                                ms.Position = blockOffset + (block.size + (2 * sizeof(UInt32)));

                                progress = ((double)ms.Position / fs.Length) * 100;
                                if (progress >= checkpoint) {
                                    UpdateProgress(progress, ref checkpoint);
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("\n");
            }

            postgreSql.CreateIndexes();
            postgreSql.RemoveStaleBlocks();
            postgreSql.AddHeightColumn();

            Console.WriteLine($"end time; {string.Format("{0:HH:mm:ss tt}", DateTime.Now)}");
        }

        private void UpdateProgress(double progress, ref int checkpoint) {

            Console.Write("\r[");

            for (int i = 0; i < (int)progress / 5; ++i) {
                Console.Write("#");
            }

            for (int i = 0; i < 20 - ((int)progress / 5); ++i) {
                Console.Write("-");
            }

            Console.Write("] ");
            Console.Write($"{checkpoint}%");
            checkpoint += 5;
        }

    }

}