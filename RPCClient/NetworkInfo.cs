using System.Collections.Generic;
using System.Dynamic;
using VCBitcoin.RPCClient;

namespace VCBitcoinRpcClient {

    public class Network {

        public string name { get; set; }
        public bool limited { get; set; }
        public bool reachable { get; set; }
        public string proxy { get; set; }
        public bool proxy_randomize_credentials { get; set; }

    }

    public class Result {

        public int version { get; set; }
        public string subversion { get; set; }
        public int protocolversion { get; set; }
        public string localservices { get; set; }
        public bool localrelay { get; set; }
        public int timeoffset { get; set; }
        public bool networkactive { get; set; }
        public int connections { get; set; }
        public List<Network> networks { get; set; }
        public double relayfee { get; set; }
        public double incrementalfee { get; set; }
        public List<object> localaddresses { get; set; }
        public string warnings { get; set; }

    }

    public class NetworkInfo : IResponseObject {

        public Result result { get; set; }
        public object error { get; set; }
        public string id { get; set; }
        public string JsonResponse { get; set; }

    }

}