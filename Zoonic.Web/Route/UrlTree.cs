using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Concurrency;

namespace Zoonic.Web.Route
{
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    public class UrlTree
    {
        protected class UrlTreeItem
        {
            public IRoute Router { get; set; }
            public IConstraint Constraint { get; set; }

            public List<UrlTreeItem> Next { get; set; } = new List<UrlTreeItem>();
            public void Add(IConstraint constraint)
            {
                Next.Add(new UrlTreeItem() { Constraint = constraint });
            }
            public void Add(UrlTreeItem treeItem)
            {
                Next.Add(treeItem);
            }
            public bool Contains(string route)
            {
                return Next.Any(m => m.Constraint.Segments.Equals(route));
            }
            public UrlTreeItem Find(string route)
            {
                return Next.Where(m => m.Constraint.Segments.Equals(route)).FirstOrDefault();
            }
            public List<UrlTreeItem> Match(string segment)
            {
                return Next.Where(m => m.Constraint.Match(segment)).ToList();
            }
        }

        protected UrlTreeItem Root { get; set; } = new UrlTreeItem();
        public void Build(string routeUrl) => Build(routeUrl, new Dictionary<string, IConstraint>()
            , new DefaultRoute());
        public void Build(string routeUrl, Dictionary<string, IConstraint> constraints) => Build(routeUrl, constraints, new DefaultRoute());
        public void Build(string routeUrl, Dictionary<string, IConstraint> constraints, IRoute template)
        {
            var segments = routeUrl.Split('/');
            CombineTree(Root,segments, constraints, template);
        }
        private UrlTreeItem CombineTree(UrlTreeItem root, string[] routekeys, Dictionary<string, IConstraint> constraints, IRoute template)
        {
            if (root == null)
            {
                return root;
            }
            if (routekeys == null || routekeys.Length == 0)
            {
                root.Router = template;
                return root;
            }
            var routekey = routekeys[0];
            UrlTreeItem tree = null;
           if (routekey.StartsWith("{")
                && routekey.EndsWith("}"))
            {
                routekey = routekey.Trim('}', '{');
            }
            if (root.Contains(routekey))
            {
                tree = root.Find(routekey);
            }
            else
            {
                tree = new UrlTreeItem();

                if (constraints.ContainsKey(routekey))
                {
                    tree.Constraint = constraints[routekey];
                }
                else
                {
                    tree.Constraint = new StringConstraint(routekey);
                }
                root.Add(tree);
            }
            return CombineTree(tree, routekeys.Skip(1).ToArray(), constraints, template);

        }

        private static UrlTree _Tree;
        public static UrlTree Tree
        {
            get
            {
                if (_Tree == null)
                {
                    _Tree = new UrlTree();
                }
                return _Tree;
            }
        }
        public IRoute Match(string url
            ,HttpContext context
            ,RouteValueDictionary routeData)
        {
            var urlSegments = url.Split('/').Where(m=>!string.IsNullOrWhiteSpace(m)).ToArray();
            if (urlSegments.Length == 0)
            {
                return new DefaultRoute();
            }
            if (!Root.Contains(urlSegments[0]))
            {
                return new DefaultRoute();
            }
            var trees = Root.Match(urlSegments[0]);
            foreach(var item in urlSegments.Skip(1))
            {
                trees = Match1(item, trees);
            }
            if (trees.Count == 1)
            {
                return trees[0].Router;
            }
            return null;

        }
        private UrlTreeItem SelectBest(List<UrlTreeItem> list,string url,string routeData
            , HttpContext context)
        {
            return null;

        }
        private List<UrlTreeItem> Match1(string segment,List<UrlTreeItem> list)
        {
            var result = new List<UrlTreeItem>();
            foreach(var item in list)
            {
                result.AddRange(item.Match(segment));
            }
            return result;
        }
    }
}
