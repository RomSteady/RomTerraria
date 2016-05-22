using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace RTHooks
{
    public class LoggingContentManager : ContentManager
    {
        private ContentManager mgr;
        private const string ContentManagerTag = "ContentManager";

        public LoggingContentManager(IServiceProvider sp, string rootDirectory)
            : base(sp, rootDirectory)
        {
        }

        public LoggingContentManager(ContentManager m) 
            : base(m.ServiceProvider, m.RootDirectory)
        {
            mgr = m;
        }

        public override T Load<T>(string assetName)
        {
            Debug.WriteLine(String.Format("Load<{0}>(\"{1}\")", typeof(T), assetName), ContentManagerTag);
            T cont = base.Load<T>(assetName);
            return cont;
        }

        protected override void Dispose(bool disposing)
        {
            Debug.WriteLine("Disposing", ContentManagerTag);
            base.Dispose(disposing);
        }

        public override void Unload()
        {
            Debug.WriteLine("Unload", ContentManagerTag);
            base.Unload();
        }
    }
}
