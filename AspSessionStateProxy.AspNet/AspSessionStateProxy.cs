using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AspSessionStateProxy.AspNet
{

    public class AspSessionStateProxy :
        MarshalByRefObject
    {

        /// <summary>
        /// Sets an item in the ASP.Net session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Push(IDictionary<string, object> items)
        {
            foreach (var kvp in items)
                HttpContext.Current.Session[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Gets the items from the ASP.Net session state.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> Pull()
        {
            return HttpContext.Current.Session.It
        }

    }

}
