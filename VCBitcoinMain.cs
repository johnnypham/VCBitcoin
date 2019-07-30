using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VCBitcoinBlockchainParser;
using VCBitcoinNetwork;
using VCBitcoinRpcClient;

namespace VCBitcoin {

    class VCBitcoinMain {

        const string ipAddress = "127.0.0.1";

        // defaults: 8332 (mainnet), 18332 (testnet)
        //const int port = 18332;
        const int port = 8332;

        const string username = "johnny";
        const string password = "notmypw";

        static RpcClient rpcClient = new RpcClient(Net.Main, ipAddress, username, password);

        public static PostgreSQL postgreSql = new PostgreSQL();

        private static void PrintMenu() {
            Console.WriteLine("Enter a block height, block hash, transaction ID or address");
            Console.WriteLine("Enter 0 to quit.");
            Console.WriteLine("Search for:");
        }

        private async static Task<QueryType> ValidInput(string input) {
            return await postgreSql.CheckInput(input);
        }

        private async static Task<string> GetResult(string input) {
            return await postgreSql.SelectHash(input);
        }

        static void Main(string[] args) {

            //VCBitcoinBlockchainParser.BlockParser parser = new VCBitcoinBlockchainParser.BlockParser();

            Console.WriteLine("VCBitcoin Block Explorer\n");
            PrintMenu();

            string input = Console.ReadLine();

            while (input != "0") {

                QueryType queryType = ValidInput(input).Result;

                switch (queryType) {

                    case QueryType.Height:
                        Console.WriteLine("tes");
                        break;

                    case QueryType.Hash:
                        string result = GetResult(input).Result;
                        if (result.Length == 0) {
                            Console.WriteLine($"Nothing found for {input}");
                        } else {
                            Console.WriteLine(result);
                        }
                        break;

                    case QueryType.InvalidHeight:
                        Console.WriteLine("Invalid block height.");
                        break;

                    default:
                        Console.WriteLine($"Nothing found for {input}");
                        break;
                }

                Console.WriteLine("");
                PrintMenu();
                input = Console.ReadLine();
            }

            Console.WriteLine("Exiting...");

            //concurrent requests
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("concurrent requests\n");
            //    concurrentRequests();
            //} catch (Exception ex) {
            //}

            ////getblockchaininfo
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("getblockchaininfo\n");

            //    // Getting the full response
            //    string response = getBlockchainInfo().Result;
            //    Console.WriteLine(response);
            //} catch (WebException ex) {
            //}

            ////getdifficulty
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("getdifficulty\n");

            //    // Getting the full response
            //    string response = getDifficulty().Result;
            //    Console.WriteLine(response);
            //} catch (WebException ex) {
            //}

            //// Accessing values without saving the response to an object
            //// getsubversion
            //try {
            //    Console.WriteLine();
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("accessing values without saving the response");
            //    Console.WriteLine("\ngetsubversion");
            //    Console.WriteLine(getSubversion().Result);
            //    Console.WriteLine("");
            //    // Accessing an array value will return a json array as a string, which can
            //    // be serialized again and further processed
            //    Console.WriteLine("accessing array values:");
            //    Console.WriteLine(getNetworks().Result);

            //} catch (AggregateException ex) {
            //    Console.WriteLine(ex.Message);
            //}

            // uptime
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("uptime\n");

            //    // Getting the response
            //    string uptimeResponse = getUptime().Result;
            //    Console.WriteLine($"uptime in seconds: {uptimeResponse}");
            //} catch (WebException ex) {
            //}

            //// getaddressinfo
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("getaddressinfo\n");

            //    // Getting the response
            //    string addressInfoResponse = getAddressInfo().Result;
            //    Console.WriteLine($"address info: {addressInfoResponse}");
            //} catch (WebException ex) {
            //}

            // estimatesmartfee
            //try {
            //    Console.WriteLine("=======================================");
            //    Console.WriteLine("estimatesmartfee\n");

            //    // Getting the response
            //    string feeResponse = getEstimateSmartFee().Result;
            //    Console.WriteLine($"feerate: {feeResponse}");
            //} catch (WebException ex) {
            //}

            //Console.WriteLine("getblockhash");
            //string genesisBlockHash = getBlockHash().Result;

            //Console.WriteLine("getblock");
            //Console.WriteLine(PrintJsonFormat(getBlock(genesisBlockHash).Result));

        }

        // prints JSON in indented format
        public static string PrintJsonFormat(string text) {
            JObject jsonResponse = JObject.Parse(text);
            return JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
        }

        // handle more than one request at a time
        public static async void concurrentRequests() {

            Console.WriteLine("first request: ");
            Console.WriteLine(await
                rpcClient.request(new[] { "getnetworkinfo", "result", "subversion" }));

            Console.WriteLine("\nsecond request: ");
            Console.WriteLine(await
                rpcClient.request(new[] { "getnetworkinfo", "result", "subversion" }));

            Console.WriteLine("");
        }

        // getblockchaininfo
        public static async Task<string> getBlockchainInfo() {
            return await rpcClient.request(new[] { "ping" });
        }

        // getblockchaininfo8
        public static async Task<string> getDifficulty() {
            return await rpcClient.request(new[] { "getdifficulty" });
        }

        // getnetworkinfo object
        public static NetworkInfo getNetworkInfoObject(string response) {
            return rpcClient.requestObject<NetworkInfo>("NetworkInfo", response);
        }

        // getsubversion 
        public static async Task<string> getSubversion() {
            return await rpcClient.request(new[] { "getnetworkinfo", "result", "subversion" });
        }

        // getnetworks
        public static async Task<string> getNetworks() {
            return await rpcClient.request(new[] { "getnetworkinfo", "result", "networks" });
        }

        // getaddressinfo
        public static async Task<string> getAddressInfo() {
            return await rpcClient.request(
                new[] { "getaddressinfo" },
                new[] { "1PSSGeFHDnKNxiEyFrD1wcEaHr9hrQDDWc" });
        }

        // estimatesmartfee
        public static async Task<string> getEstimateSmartFee() {
            return await rpcClient.request(
                new[] { "estimatesmartfee", "result", "feerate" },
                new[] { "2", "ECONOMICAL" });
        }

        // getblockhash
        public static async Task<string> getBlockHash() {
            return await rpcClient.request(
                new[] { "getblockhash", "result" },
                new[] { "0" });
        }

        // getblock
        public static async Task<string> getBlock(string headerHash) {
            return await rpcClient.request(
                new[] { "getblock" },
                new[] { headerHash, "2" });
        }

    }

}
