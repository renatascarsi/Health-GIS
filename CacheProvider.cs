using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    public class CacheProvider
    {
        #region Static Properties

        public static Cache Instance
        {
            get
            {
                return HttpRuntime.Cache;
            }
        }

        #endregion
    }
}