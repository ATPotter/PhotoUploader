using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;
using CookComputing.XmlRpc;
using PictureTagger.ExifToolWrapper;
using Wordpress;
using Wordpress.Xml;
using Wordpress.Xml.Rpc;

using xmlrpc;

namespace PhotoUploader
{
    public struct blogInfo
    {
        public string title;
        public string description;
        public DateTime dateCreated;
    }

    public struct newMediaDescriptor
    {
        public string name;
        public string type;
        public string bits;
        public bool overwrite;
    };

    public struct newMediaResponse
    {
        public string id;
        public string file;
        public string url;
        public string type;
    };

    public interface IgetCatList
    {
        [CookComputing.XmlRpc.XmlRpcMethod("metaWeblog.newPost")]
        string NewPost(int blogId, string strUserName,
            string strPassword, blogInfo content, int publish);

        [CookComputing.XmlRpc.XmlRpcMethod("metaWeblog.newMediaObject")]
        newMediaResponse NewMedia(int blogId,
            string username,
            string password,
            newMediaDescriptor data);
    }


    public partial class Form1 : Form
    {

        public XmlRpcClientProtocol clientProtocol;
        public IgetCatList categories;


        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Username:    test
            // Password:    DjF3iUfMDto0
            blogInfo newBlogPost = default(blogInfo);
            newBlogPost.title = tbTitle.Text;
            newBlogPost.description = tbBody.Text;
            newBlogPost.dateCreated = new DateTime(2009,10,11);

            categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            clientProtocol = (XmlRpcClientProtocol)categories;
            clientProtocol.Url = "http://www.thepotters.org/test/wpl/xmlrpc.php";
            string result = null;
            result = "";
            try
            {
                result = categories.NewPost(1, "test", "DjF3iUfMDto0", newBlogPost, 1); 
                MessageBox.Show("Posted to Blog successfullly! Post ID : " + result); 
                tbBody.Text = "";
                tbTitle.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnImg_Click(object sender, EventArgs e)
        {
            var fi = new FileInfo("C:\\Program Files\\Utilities\\exiftool.exe");
            var exifTool = new ExifToolWrapper(fi);

            // Build our list of desired tags
            var tagList = new List<string>();
            tagList.AddRange(new String[]
            {
                "Title",
                "Description",
                "DateTimeOriginal"
            });

            FileInfo imageFi = new FileInfo(lblImage.Text);
            var tagValues = exifTool.GetTagsFromFile(imageFi, tagList);




            var newMediaPost = default(newMediaDescriptor);
            newMediaPost.name = "ProgramaticallyUploadedImage.jpg";
            newMediaPost.type = "image/jpeg";
            newMediaPost.overwrite = false;
            
            // Now we read in the picture and turn it into a byte array
            byte[] fileAsBytes;

            using (var fs = new FileStream(lblImage.Text, FileMode.Open))
            {
                fileAsBytes = new byte[fs.Length];
                fs.Read(fileAsBytes, 0, (int)fs.Length);
            }

            // And convert it to base-64 encoding
            newMediaPost.bits = Convert.ToBase64String(fileAsBytes);



            categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            clientProtocol = (XmlRpcClientProtocol)categories;
            clientProtocol.Url = "http://www.thepotters.org/test/wpl/xmlrpc.php";

            try
            {
                var result = categories.NewMedia(
                    3, "test", "DjF3iUfMDto0", newMediaPost);


                MessageBox.Show(string.Format(
                    "ID: {0}\r\nFile: {1}\r\nURL: {2}\r\nType:{3}",
                    result.id,
                    result.file,
                    result.url,
                    result.type));

                tbBody.Text = "";
                tbTitle.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        
        }
    }
}
