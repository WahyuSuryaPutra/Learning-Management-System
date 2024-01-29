using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;

namespace GLALMS
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btn_Click(object sender, EventArgs e)
        {
            string conn2 = ConfigurationManager.ConnectionStrings["Myconn"].ConnectionString;
            SqlConnection con = new SqlConnection(conn2);
            con.Open();
            SqlCommand cmd = new SqlCommand("select email,password from Student where email='" + email.Text + "'and password='" + Password.Text + "'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                Response.Write("<script language=javascript>alert('Login successfully Done');</script>");
                Response.Redirect("Home.aspx");
                //MessageBox.Show("Login sucess Welcome to Homepage http://krishnasinghprogramming.nlogspot.com");
                //System.Diagnostics.Process.Start("http://krishnasinghprogramming.blogspot.com");
            }
            else
            {
                Response.Write("<script language=javascript>alert('User Name and password are mismatched');</script>");
                // MessageBox.Show("Invalid Login please check username and password");
            }
            con.Close();
        }
    }
}