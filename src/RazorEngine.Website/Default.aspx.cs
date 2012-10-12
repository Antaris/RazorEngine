using System;

namespace RazorEngine.Website
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //Basic Parsing Test
                const string template = "<div>Hello @Model.Name</div>";
                var model = new { Name = "World" };
                Body.Text = Razor.Parse(template, model);
            }
            catch (Exception exception)
            {

                Response.Write(exception.Message + " " + exception.StackTrace);

                if (exception.InnerException != null)
                {
                    Response.Write(exception.InnerException.Message + " " + exception.InnerException.StackTrace);
                }
            }
        }
    }
}
