using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace KBlog_WPF
{
    public class User_DB_Query
    {
        SqlConnection con = new SqlConnection("Server=localhost;database=KBlog; trusted_connection= true");

        public string get_user_name(string user_id)
        {
            string first_name, last_name, full_name;
            full_name = "";
            using (con)
            {
                
                con.Open();
                SqlCommand cmd = new SqlCommand("get_user_full_name", con);
                cmd.CommandType = CommandType.StoredProcedure;
                //Create PROCEDURE [dbo].[get_user_full_name]
                //@user_id bigint
                //AS
                //BEGIN
                //    Select first_name, last_name from Users where user_id=@user_id
                //END
                cmd.Parameters.AddWithValue("@user_id", user_id);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    first_name = rdr["first_name"].ToString();
                    last_name = rdr["last_name"].ToString();
                    full_name = first_name + " " + last_name;
                }

            }
            return full_name;


        }
        
        public bool validate_guid(string user_guid)
        {
            bool result = true;
            using (con)
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("validate_user_guid", con);
                //Create PROCEDURE validate_user_guid
                //@user_guid varchar(80)
                //AS
                //BEGIN
                //    if exists (Select * from Users where user_guid=@user_guid )
                //        BEGIN
                //        Select 1 as ReturnCode
                //        Select user_id from Users where user_guid=@user_guid
                //        END
                //    else
                //        Select 0 as ReturnCode
                //END
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@user_guid", user_guid);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (Convert.ToBoolean(rdr["ReturnCode"]))
                    {
                        result = true;
                        rdr.NextResult();
                        while (rdr.Read())
                        { 
                            ApplicationState.SetValue("user_id", rdr["user_id"].ToString()); 
                        }
                        
                    }
                    else
                    {
                        result = false;
                    }
                }

            }
            return result;
        }
    }
}
