using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.Web.Script.Services;
using System.Data.Entity;
using System.Data;
using AICC_CMI;
using EntityFrameworkLayer;

namespace LMS_Prototype_1
{
    /// <summary>
    /// AICC_SCORM Web Service
    /// </summary>
    [WebService(Namespace = "http://compliancefactors.com")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class AICC_SCORM : WebService
    {
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PostCMI(string cmi_dto)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            dynamic cmi = jss.Deserialize(cmi_dto, typeof(object)) as dynamic;

            Dictionary<string, string> cmi_dict = cmi.Flatten();           

            /*
                cmi.comments
                cmi.comments
                cmi.comments_from_lms
            */
            
            //var test = cmi["interactions_"+0].id;

//            var interactions_count = cmi.interactions._count;
           
            string enrollment_id = cmi.enrollment_id;

            JS_API_Logic js_api = new JS_API_Logic();
            js_api.ConsumeJSObj(cmi_dict);
            js_api.Persist(enrollment_id);

            return "";
        }

    }
    public class DynamicJsonObject : DynamicObject
    {
        private IDictionary<string, object> Dictionary { get; set; }

        public DynamicJsonObject(IDictionary<string, object> dictionary)
        {
            this.Dictionary = dictionary;
        }

        public Dictionary<string, string> Flatten()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            WalkDict("cmi", this.Dictionary, ret);

            return ret;
        }

        private void WalkDict(string parent_key, IDictionary<string, object> input, Dictionary<string, string> output)
        {
            foreach (KeyValuePair<string,object> pair in input)
            {
                if (pair.Value is Dictionary<string, object>)
                    WalkDict(parent_key + "." + pair.Key, (Dictionary<string, object>) pair.Value, output);
                else
                    output[parent_key + "." + pair.Key] = (pair.Value).ToString();
            }
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.Dictionary[binder.Name];

            if (result is IDictionary<string, object>)
            {
                result = new DynamicJsonObject(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                var list = new List<object>();
                foreach (var o in (result as ArrayList).ToArray())
                {
                    if (o is IDictionary<string, object>)
                    {
                        list.Add(new DynamicJsonObject(o as IDictionary<string, object>));
                    }
                    else
                    {
                        list.Add(o);
                    }
                }
                result = list;
            }

            return this.Dictionary.ContainsKey(binder.Name);
        }
    }

    public class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(object))
            {
                return new DynamicJsonObject(dictionary);
            }

            return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
        }
    }
}
