using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace KBlog_WPF
{
    public class Media_DB_Query
    {
        SqlConnection con = new SqlConnection("Server=localhost;database=KBlog; trusted_connection= true");

        public void saveMedia(string user_id, string media_description, string dir_path)
        {
            SqlCommand cmd = new SqlCommand("usp_insert_media", con);
            cmd.CommandType = CommandType.StoredProcedure;
            //Create Procedure usp_insert_media
            //@media_description varchar(80),
            //@user_id bigint, 
            //@dir_path varchar(max)
            //AS
            //BEGIN
            //    DECLARE @media_type_id bigint
            //    SET @media_type_id=(Select media_type_id from Media_Type where description=@media_description)
            //    INSERT INTO Media(user_id, date, media_type_id, dir_path) VALUES (@user_id, getdate(), @media_type_id, @dir_path)
            //END
            cmd.Parameters.AddWithValue("@user_id", user_id);
            cmd.Parameters.AddWithValue("@media_description", media_description);
            cmd.Parameters.AddWithValue("@dir_path", dir_path);
            using (con)
            {
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex) 
                {
                    throw ex;
                }
            }
        }
    }
}
