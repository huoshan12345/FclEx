using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FclEx.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FclEx
{
    public static class XmlExtensions
    {
        public static XElement RemoveComment(this XElement xml)
        {
            xml.DescendantNodes().OfType<XComment>().Remove();
            return xml;
        }

        public static string ToJson(this XNode xml, 
            Formatting formatting = Formatting.None, 
            bool omitRootObject = false)
        {
            return JsonConvert.SerializeXNode(xml);
        }

        public static string ToJson(this XmlNode xml,
            Formatting formatting = Formatting.None,
            bool omitRootObject = false)
        {
            return JsonConvert.SerializeXmlNode(xml);
        }

        public static TreeNode<XElement> ToTree(this XElement xml)
        {
            var root = new TreeNode<XElement>(xml);
            var map = new Dictionary<XElement, TreeNode<XElement>>()
            {
                { xml, root }
            };
            var queue = new Queue<XElement>();
            queue.Enqueue(xml);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                var node = map[item];
                item.Ancestors().ForEach(m =>
                {
                    var child = node.AddChild(m);
                    queue.Enqueue(m);
                    map.Add(m, child);
                });
            }
            return root;
        }
    }
}
