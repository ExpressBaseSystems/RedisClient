using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using ServiceStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using EbRedisClient;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EbControllers
{
    public class EbRedisManagerController : Controller
    {

        
        static readonly string redisConnectionString = string.Format("connectionstring");
        RedisClient _redis = new RedisClient(redisConnectionString);
        // GET: /<controller>/
        public IActionResult Index()
        {
            List<string> cmdlst = new List<string>();
            Type t = typeof(Commands);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fi in fields)
            {
                cmdlst.Add(fi.Name.ToString());

            }
            ViewBag.cmdbag = cmdlst;
            List<EbGroup> lst = new List<EbGroup>();
            List<String> lst2 = new List<String>();
            foreach (var m in _redis.GetKeysByPattern("Grp_*"))
            {
                var a = _redis.Get<string>(m);
                EbGroup obj = JsonConvert.DeserializeObject<EbGroup>(a);
                lst.Add(obj);

            }
            foreach (var n in _redis.GetAllKeys())
            {
                lst2.Add(n);
            }
            ViewBag.tlst = lst;
            ViewBag.klist = JsonConvert.SerializeObject(lst2);
            return View();
        }



        Dictionary<object, object> dict = new Dictionary<object, object>();
        List<string> list1 = new List<string>();
        public List<string> FindMatch(string text)
        {
            foreach (var m in _redis.GetKeysByPattern(text))

            {
                list1.Add(m);
            }
            return list1;
        }


        public bool Keydeletes(string textdel)
        {
            if (_redis.ContainsKey(textdel))
            {
                _redis.Remove(textdel);
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool Keyvalueinput(string textkey, string textvalue)
        {
            bool b;
            long val = _redis.Exists(textkey);
            if (val >= 1) { return false; }
            if ((textkey != "") && (textvalue != ""))
            {
                _redis.Set(textkey, textvalue);
                b = true;
            }
            else { b = false; }
            return b;

        }
        public List<string> Allkeys(object textallkeys)
        {
            List<String> lst3 = new List<String>();
            lst3 = _redis.GetAllKeys();
            return lst3;
        }

        public List<string> FindRegexMatch(string textregex)
        {
            var base64EncodedBytes = Convert.FromBase64String(textregex);
            var regx = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            Regex rgx = new Regex(regx);
            foreach (var m in _redis.GetAllKeys())
            {
                if (rgx.IsMatch(m))
                    list1.Add(m);
            }
            return list1;
        }




        public void GroupPattern(string textgroup, string textpattern)
        {
            var base64EncodedBytes = Convert.FromBase64String(textpattern);
            var regx = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

            EbGroup grp = new EbGroup(textgroup, regx);

            string output = JsonConvert.SerializeObject(grp);
            _redis.Set("Grp_" + textgroup, output);

            EbGroup dobj = JsonConvert.DeserializeObject<EbGroup>(output);
            string s1 = dobj.Pattern;
            string s2 = dobj.Name;

        }



        public object FindVal(string key_name)
        {

            object val = null;
            var type = _redis.Type(key_name);

            if (type == "string")
                val = _redis.Get<string>(key_name);
            else if (type == "list")
                val = _redis.GetAllItemsFromList(key_name);
            else if (type == "hash")
            {
                val = _redis.GetAllEntriesFromHash(key_name);
            }
            else if (type == "set")
            {
                val = _redis.GetAllItemsFromSet(key_name);
            }
            else if (type == "zset")
            {
                // val = _redis.GetAllItemsFromSortedSet(key_name);
               val= _redis.GetAllWithScoresFromSortedSet(key_name);
            }
            return new FindValClass { Key = key_name, Obj = val, Type = type };

        }


        public void ListInsert(string txtlistkey, string txtlistval, int flag)
        {
            if (flag == 1)
            {
                var lpushval = Encoding.UTF8.GetBytes(txtlistval);
                _redis.LPush(txtlistkey, lpushval);
            }
            if (flag == 2)
            {
                var rpushval = Encoding.UTF8.GetBytes(txtlistval);
                _redis.RPush(txtlistkey, rpushval);
            }
        }

        public void HashInsert(string txthashkey, string txthashfield, string txthashval)
        {
            var hashfield = Encoding.UTF8.GetBytes(txthashfield);
            var hashval = Encoding.UTF8.GetBytes(txthashval);
            _redis.HSet(txthashkey, hashfield, hashval);
        }
        public void SetInsert(string txtsetkey, string txtsetval)
        {
            var sethval = Encoding.UTF8.GetBytes(txtsetval);
            _redis.SAdd(txtsetkey, sethval);
        }

        public void SortedsetInsert(string txtzsetkey, string txtzsetscr, string txtzsetval)
        {
            double scor = Convert.ToDouble(txtzsetscr);
            var zsethval = Encoding.UTF8.GetBytes(txtzsetval);
            _redis.ZAdd(txtzsetkey, scor, zsethval);
        }
        public bool Renamekey(string oldkey, string newkey)
        {
            bool b;
            if (_redis.ContainsKey(newkey))
                b = false;
            else
            {
                _redis.RenameKey(oldkey, newkey);
                b = true;
            }
            return b;
        }
        public bool StringvalEdit(string key1, string value1)
        {
            bool bl;
            if ((key1 != "") && (value1 != ""))
            {
                _redis.SetValue(key1, value1);
                bl = true;
            }
            else bl = false;
            return bl;

        }
        public void ListValEdit(string l_keyid, string dict)
        {
            Dictionary<int, string> val = JsonConvert.DeserializeObject<Dictionary<int, string>>(dict);

            int v = Convert.ToInt32(_redis.LLen(l_keyid));
            int i;
            for (i = v; i < val.Count; i++)
            {
                var lv = Encoding.UTF8.GetBytes(val[i]);
                _redis.RPush(l_keyid, lv);

            }

            foreach (var item in val)
            {
                var listval = Encoding.UTF8.GetBytes(item.Value);
                _redis.LSet(l_keyid, item.Key, listval);
            }
        }
        public void SetValEdit(string l_keyid, string dict)
        {
            Dictionary<string, string> val = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict);
           
                foreach (var item in val)
                {
                    var setval = Encoding.UTF8.GetBytes(item.Value);
                    _redis.SAdd(l_keyid, setval);
                }
            
        }


        public void HashValEdit(string h_keyid, string dict)
        {
            Dictionary<string, string> val = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict);
            var tp = _redis.Type(h_keyid);
            if (tp == "zset")
            {
                foreach (var item in val)
                {
                    double scor = Convert.ToDouble(item.Value);
                    var zsethval = Encoding.UTF8.GetBytes(item.Key);
                    _redis.ZAdd(h_keyid, scor, zsethval);
                }
            }
            if (tp == "hash")
            {
                foreach (var item in val)
                {
                    var field = Encoding.UTF8.GetBytes(item.Key);
                    var v = Encoding.UTF8.GetBytes(item.Value);
                    _redis.HSet(h_keyid, field, v);
                }
            }

        }
        public object Terminal(string cmd)
        {

            try
            {

                object[] arr = cmd.Split(" ");
                RedisText rd = new RedisText();
                rd = _redis.Custom(arr);
                if (rd.Text == null)
                {
                    List<string> terminal_list = new List<string>();
                    foreach (var child in rd.Children)
                    {
                        terminal_list.Add(child.Text);
                    }

                    return terminal_list;

                }
                else
                    return rd.Text;
            }

            catch (Exception e) { return e.Message; }
        }
    }
}
