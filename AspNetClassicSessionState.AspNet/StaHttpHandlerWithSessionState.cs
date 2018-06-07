using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace AspNetClassicSessionState.AspNet
{
    /// <summary>
    /// This handler allows SOAP Web Services to run in STA mode
    /// 
    /// Note: this will NOT work with ASP.NET AJAX services as
    /// these services bypass the handler mapping and perform
    /// all work in a module (RestModule).
    /// </summary>
    public abstract class StaHttpHandler :
        System.Web.UI.Page, IHttpAsyncHandler
    {
        protected override void OnInit(EventArgs e)
        {
            this.ProcessRequestSta(this.Context);

            // immediately stop the request after we're done processing
            this.Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// This actually should never fire but has to be here for IHttpHandler
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            // internally runs async request so Begin/EndProcessRequest are called
            base.ProcessRequest(context);
        }

        /// <summary>
        /// This method should receive the real processing code
        /// </summary>
        /// <param name="context"></param>
        public abstract void ProcessRequestSta(HttpContext context);


        /// <summary>
        /// This is what handles the STA thread creation/pool thread queing
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        public IAsyncResult BeginProcessRequest(
            HttpContext context, AsyncCallback cb, object extraData)
        {
            return this.AspCompatBeginProcessRequest(context, cb, extraData);
        }


        public void EndProcessRequest(IAsyncResult result)
        {
            this.AspCompatEndProcessRequest(result);
        }
    }

    /// <summary>
    /// Subclass this if your handler needs session state
    /// </summary>
    public class StaHttpHandlerWithSessionState :
                    StaHttpHandler, IRequiresSessionState
    {
        public override void ProcessRequestSta(HttpContext context)
        {
        }

    }
}
