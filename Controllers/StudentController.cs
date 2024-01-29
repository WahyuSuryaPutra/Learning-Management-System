using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using LMS.Models;
using System;

namespace LMS.Controllers
{
    public class StudentController : Controller
    {
        string DataBase = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString;

        public ActionResult Login2()
        {
            return View();
        }
        public ActionResult Index()
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Student");
            }
            else
            {


                //

                if (TempData["234"] != null)
                {
                string stt = TempData["234"].ToString();
                ViewBag.Stu = stt;
                }
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql = "SELECT (SELECT COUNT(*) FROM Docs_Diploma_CS WHERE Type = " +
                        "'Assignment' ), (SELECT COUNT(*) FROM Assignments WHERE studentID = @sID)";


                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@sID", Session["ID"]);

                        SqlDataReader rd = cmd.ExecuteReader();

                        while (rd.Read())
                        {
                            ViewBag.Total = rd.GetInt32(0);
                            ViewBag.Done = rd.GetInt32(1);
                        }

                        rd.Close();
                    }

                    string sql2 = "Select marks from Assignments WHERE studentID = @sID AND marks != 'NaN'";

                    double tResult = 0;
                    double gResult = 0;

                    using(SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@sID", Session["ID"]);
                        SqlDataReader rd = cmd.ExecuteReader();

                        while (rd.Read())
                        {
                            string r = rd.GetString(0);
                            gResult += Convert.ToDouble(r.Split('/')[0].Trim());
                            tResult += Convert.ToDouble(r.Split('/')[1].Trim());
                        }

                        rd.Close();
                    }

                    double result = (gResult / tResult) * 100;
                    result = Math.Round(result, 2);
                    ViewBag.Result = result;


                }

                ViewData["assignmentStatus"] = TempData["aStatus"];
                return View();
            }
        }

        public ActionResult Login()
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult Login(FormCollection collection, Models.Login log)
        {
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "SELECT * FROM Students WHERE uname = @uname and password = @password";
                    string sql2 = "UPDATE Students SET lastlogin = CURRENT_TIMESTAMP WHERE studentID = @ID";
                    int n = 0;
                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@uname", log.uname);
                        cmd.Parameters.AddWithValue("@password", log.password);
                        SqlDataReader rd = cmd.ExecuteReader();

                        String st = cmd.CommandText;

                        TempData["234"] = st;

                        while (rd.Read())
                        {
                            if (rd.GetString(2) == log.uname && rd.GetString(4) == log.password)
                            {
                                Session["uname"] = rd.GetString(2);
                                Session["Name"] = rd.GetString(3);
                                Session["ID"] = rd.GetInt32(0);
                                Session["Roll"] = rd.GetInt32(1);
                                Session["Course"] = rd.GetString(6);
                                Session["Branch"] = rd.GetString(7);
                                n++;
                            }
                        }
                        rd.Close();

                    }
                    if (n != 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql2, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", Session["ID"]);
                            cmd.ExecuteNonQuery();
                        }

                        string sql3 = "select * from " + Session["Course"] + "_" + Session["Branch"] + " where ParentID = 0";
                        using (SqlDataAdapter ad = new SqlDataAdapter(sql3, conn))
                        {
                            ad.Fill(dataTable);
                        }
                        string sql4 = "select * from " + Session["Course"] + "_" + Session["Branch"];
                        using (SqlDataAdapter ad = new SqlDataAdapter(sql4, conn))
                        {
                            ad.Fill(dataTable1);
                        }

                        Session.Add("sliderItems", dataTable);
                        Session.Add("sliderItems1", dataTable1);

                        return RedirectToAction("Index", "Student");

                    }
                    else
                    {
                        Session["uname"] = null;
                        Session["uid"] = null;
                        Session["Name"] = null;
                        ViewBag.Status = "failed";
                        return View();
                    }
                }

            }
        }

        public ActionResult Logout()
        {
            Session["uname"] = null;
            Session["uid"] = null;
            Session["Name"] = null;
            Session["Roll"] = null;
            Session["Course"] = null;
            Session["Branch"] = null;
            return RedirectToAction("Login", "Student");

        }

        public ActionResult LockScreen()
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return View();
            }
        }
        public ActionResult Download(string path)
        {
            string fileName = Path.GetFileName(path);
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + (fileName));
            Response.WriteFile(path);
            Response.End();
            return RedirectToAction("Index");
        }

        public ActionResult Subject(int subId = 0, string subName = "none")
        {
            if (Session["uname"] != null)
            {
                List<string> facultyDetials = new List<string>();
                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();
                DataTable dataTable2 = new DataTable();
                DataTable uploads = new DataTable();


                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "select * from Faculty where Subjects like @sub";
                    string sql2 = "select * from Docs_Diploma_CS where SubjectID = @id AND Type = @dType";


                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@sub", ("%" + subName + "%"));
                        int n = 0;
                        SqlDataReader rd = cmd.ExecuteReader();
                        while (rd.Read())
                        {
                            n++;
                            facultyDetials.Add(rd.GetString(1));
                            facultyDetials.Add(rd.GetString(9));
                            facultyDetials.Add(rd.GetString(2));
                            facultyDetials.Add(rd.GetString(10));
                            facultyDetials.Add(rd.GetString(11));
                        }
                        rd.Close();
                        if (n > 0)
                        {
                            ViewBag.FacD = "no faculty found";
                        }
                        else
                        {
                            ViewBag.FacD = "faculty found";
                        }
                        ViewData["Faculty"] = facultyDetials;
                    }



                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Assignment");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable);
                        ViewData["Assignments"] = dataTable;
                    }



                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Notes");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable1);
                        ViewData["Notes"] = dataTable1;
                    }



                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Syllabus");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable2);
                        ViewData["Syllabus"] = dataTable2;
                    }


                    if (dataTable.Rows.Count > 0)
                    {
                        List<int> uploadedAssignent = new List<int>();
                        string sql3 = "SELECT documentID FROM Assignments WHERE studentID = @sID";
                        using (SqlCommand cmd = new SqlCommand(sql3, conn))
                        {
                            uploadedAssignent.Add(0);
                            cmd.Parameters.AddWithValue("@sID", Convert.ToInt32(Session["ID"]));
                            SqlDataReader rd = cmd.ExecuteReader();
                            while (rd.Read())
                            {
                                uploadedAssignent.Add(rd.GetInt32(0));
                            }
                            rd.Close();
                        }
                        ViewData["uploadedAssignments"] = uploadedAssignent;
                    }

                    string sql4 = "SELECT * FROM Assignments WHERE studentID = '" + Session["ID"] + "'";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql4, conn))
                    {
                        ad.Fill(uploads);
                    }
                    ViewData["uploads"] = uploads;




                }
                ViewBag.Name = subName;
                return View();

            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        [HttpPost]
        public ActionResult Subject(Asignment model)
        {
            string msg = "Hello World!!";
            if (model.assgnmentFile != null && model.assgnmentFile.ContentLength > 0)
            {
                try
                {
                    string ext = Path.GetExtension(model.assgnmentFile.FileName);
                    DateTime cur = DateTime.Now;
                    string dtt = cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString() + cur.Hour.ToString() + cur.Minute.ToString() + cur.Second.ToString();
                    string path = Path.Combine("D:/LMS/Student/Assignment", dtt + ext);


                    using (SqlConnection conn = new SqlConnection(DataBase))
                    {
                        conn.Open();
                        string sql1 = "INSERT INTO Assignments ([documentID], [path], [submit Date], [studentID]) " +
                                       "VALUES (@docID, @path, CURRENT_TIMESTAMP, @studentID)";
                        using (SqlCommand cmd = new SqlCommand(sql1, conn))
                        {
                            cmd.Parameters.AddWithValue("@docID", model.docsID);
                            cmd.Parameters.AddWithValue("@path", path);
                            cmd.Parameters.AddWithValue("@studentID", Convert.ToInt32(Session["ID"]));

                            cmd.ExecuteNonQuery();
                        }
                        model.assgnmentFile.SaveAs(path);
                        msg = "0xSSCCFF";
                    }
                }
                catch (Exception ex)
                {
                    msg = "0xEECCFF";
                    msg = ex.Message;
                }
            }
            else
            {
                msg = "0xNFCCFF";
            }
            TempData["aStatus"] = msg;
            return RedirectToAction("Index");
        }
    }
}
