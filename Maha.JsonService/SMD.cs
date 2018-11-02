using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Maha.JsonService
{
    public class SMD
    {
        public string transport { get; set; }
        public string envelope { get; set; }
        public string target { get; set; }
        public bool additonalParameters { get; set; }
        public SMDAdditionalParameters[] parameters { get; set; }

        [JsonIgnore]
        public static List<string> TypeHashes { get; set; }
        [JsonProperty("types")]
        public static Dictionary<int, JObject> Types { get; set; }
        [JsonProperty("services")]
        public Dictionary<string, SMDService> Services { get; set; }

        public SMD()
        {
            transport = "POST";
            envelope = "URL";
            target = "/json.rpc";
            additonalParameters = false;
            parameters = new SMDAdditionalParameters[0];
            Services = new Dictionary<string, SMDService>();
            Types = new Dictionary<int, JObject>();
            TypeHashes = new List<string>();
        }

        public void AddService(string method, Dictionary<string, Type> parameters, Dictionary<string, object> defaultValues)
        {
            var newService = new SMDService(transport, "JSON-RPC-2.0", parameters, defaultValues);
            Services.Add(method, newService);
        }

        public static int AddType(JObject jsonObject)
        {
            var hash = "t_" + jsonObject.ToString().GetHashCode();
            lock (TypeHashes)
            {
                var idx = 0;
                if (TypeHashes.Contains(hash) == false)
                {
                    TypeHashes.Add(hash);
                    idx = TypeHashes.IndexOf(hash);
                    Types.Add(idx, jsonObject);
                }
            }
            return TypeHashes.IndexOf(hash);
        }
    }

    public class SMDService
    {
        public SMDService(string transport, string envelope, Dictionary<string, Type> parameters, Dictionary<string, object> defaultValues)
        {
            this.transport = transport;
            this.envelope = envelope;
            this.parameters = new SMDAdditionalParameters[parameters.Count - 1]; 
            int ctr = 0;
            foreach (var item in parameters)
            {
                if (ctr < parameters.Count - 1)
                {
                    this.parameters[ctr++] = new SMDAdditionalParameters(item.Key, item.Value);
                }
            }

            this.defaultValues = new ParameterDefaultValue[defaultValues.Count];
            int counter = 0;
            foreach (var item in defaultValues)
            {
                this.defaultValues[counter++] = new ParameterDefaultValue(item.Key, item.Value);
            }

            this.returns = new SMDResult(parameters.Values.LastOrDefault());
        }
        public string transport { get; private set; }
        public string envelope { get; private set; }
        public SMDResult returns { get; private set; }

        public SMDAdditionalParameters[] parameters { get; private set; }

        public ParameterDefaultValue[] defaultValues { get; private set; }
    }

    public class SMDResult
    {
        [JsonProperty("__type")]
        public int Type { get; private set; }
        [JsonIgnore()]
        public Type ObjectType { get; set; }

        public SMDResult(System.Type type)
        {
            Type = SMDAdditionalParameters.GetTypeRecursive(type);
            ObjectType = type;
        }
    }

    public class ParameterDefaultValue
    {
        public string Name { get; private set; }

        public object Value { get; private set; }

        public ParameterDefaultValue(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class SMDAdditionalParameters
    {
        [JsonIgnore()]
        public Type ObjectType { get; set; }
        [JsonProperty("__name")]
        public string Name { get; set; }
        [JsonProperty("__type")]
        public int Type { get; set; }

        public SMDAdditionalParameters(string parametername, System.Type type)
        {
            Name = parametername;
            Type = GetTypeRecursive(ObjectType = type);
        }

        internal static int GetTypeRecursive(Type t)
        {
            JObject jo = new JObject();
            jo.Add("__name", t.Name.ToLower());

            return SMD.AddType(jo);
        }

        internal static bool isSimpleType(Type t)
        {
            var name = t.FullName.ToLower();

            if (name.Contains("newtonsoft")
                || name == "system.sbyte"
                || name == "system.byte"
                || name == "system.int16"
                || name == "system.uint16"
                || name == "system.int32"
                || name == "system.uint32"
                || name == "system.int64"
                || name == "system.uint64"
                || name == "system.char"
                || name == "system.single"
                || name == "system.double"
                || name == "system.boolean"
                || name == "system.decimal"
                || name == "system.float"
                || name == "system.numeric"
                || name == "system.money"
                || name == "system.string"
                || name == "system.object"
                || name == "system.type"
                // || name == "system.datetime"
                || name == "system.reflection.membertypes")
            {
                return true;
            }

            return false;
        }
    }
}
