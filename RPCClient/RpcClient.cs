using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VCBitcoin.RPCClient;
using VCBitcoinNetwork;

namespace VCBitcoinRpcClient {

    public class RpcClient {

        private readonly NodeConnection node;

        public RpcClient(Net netInfoType, string ipAddress, string username, string password) {
            node = new NodeConnection(netInfoType, ipAddress, username, password);
        }

        public RpcClient(Net netInfoType, string ipAddress,
            int port, string username, string password) {
            node = new NodeConnection(netInfoType, ipAddress, port, username, password);
        }

        private byte[] createJsonRequest(string methodName, string[] args) {

            JObject jsonObj = new JObject();

            jsonObj.Add(new JProperty("jsonrpc", "1.0"));
            jsonObj.Add(new JProperty("id", "1"));

            jsonObj.Add(new JProperty("method", methodName));

            if (args == null || args.Length == 0) {
                jsonObj.Add(new JProperty("params", new JArray()));
            } else {
                JArray argsArray = new JArray();

                int tempArg;

                for (int i = 0; i < args.Length; ++i) {
                    if (int.TryParse(args[i], out tempArg)) {
                        argsArray.Add(tempArg);
                    } else {
                        argsArray.Add(args[i]);
                    }
                }

                jsonObj.Add(new JProperty("params", argsArray));
            }

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonObj));
        }

        private string sendRequest(byte[] requestByteSequence) {

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(node.SocketAddress);
            webRequest.Credentials = new NetworkCredential(node.Username, node.Password);

            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";

            webRequest.ContentLength = requestByteSequence.Length;

            webRequest.GetRequestStream().Write(requestByteSequence, 0, requestByteSequence.Length);

            try {
                using (WebResponse webResponse = webRequest.GetResponse()) {
                    using (Stream stream = webResponse.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            // Read the response stream and return it as a Task<string>
                            return reader.ReadToEnd();
                        }
                    }
                }
            } catch (WebException ex) {
                Console.WriteLine(ex.Message);
                throw;
            } catch (Exception ex) {
                throw;
            }
        }

        public Task<string> request(string[] keys, string[] args = null) {

            if (keys == null || keys.Length == 0) {
                return Task.FromResult("request() needs at least one argument.");
            }

            byte[] requestByteSequence = createJsonRequest(keys[0], args);

            string response = sendRequest(requestByteSequence);

            if (keys.Length == 1) {
                return Task.FromResult(response);
            }

            if (keys.Length > 1) {
                for (int i = 1; i < keys.Length; ++i) {
                    JObject temp = JObject.Parse(response);

                    if (temp.ContainsKey(keys[i])) {
                        response = temp[keys[i]].ToString();
                    } else {
                        response = $"{keys[i]}: key not found";
                    }
                }

                return Task.FromResult(response);
            }

            return Task.FromResult($"{keys[0]}: method not found");
        }

        public T requestObject<T>(string className, string jsonResponse) where T : IResponseObject {

            Type givenClassType = Type.GetType($"VCBitcoinRpcClient.{className}");

            if (givenClassType != null) {
                object instance = Activator.CreateInstance(givenClassType);

                PropertyInfo prop = givenClassType.GetProperty("JsonResponse");

                if (prop != null) {
                    prop.SetValue(instance, jsonResponse, null);

                    dynamic givenClass = JsonConvert.DeserializeObject<T>(jsonResponse);

                    givenClass.JsonResponse = prop.GetValue(instance, null) as string;

                    return givenClass;
                }
            } else {
                throw new ArgumentException($"{className}: class not found.");
            }
            return default;
        }

    }

}