using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LegalaffairsSOP
{
    class Program
    {
        // --- 初始化 DB Model 
        private static SOPDataDataContext sop_datacontent = new SOPDataDataContext();

        static void Main(string[] args)
        {
            //--- 建立 webclient
            using (WebClient webClient = new WebClient())
            {
                // --- 撈取資料表SOP資料
                var sopdata = sop_datacontent.tdsopdata.ToList();
                var tdsopdata = new tdsopdata();
                // --- 指定 WebClient 的編碼
                webClient.Encoding = Encoding.UTF8;
                // --- 指定 WebClient 的 Content-Type header
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                // --- 從網路 url 上取得法務局SOP資料
                var url = "https://tplaw.blob.core.windows.net/blobfs/SOP";
                // --- 將法務局SOP資料轉成字串
                var SOPDataList = webClient.DownloadString(url);
                // --- 將字串轉為object 
                dynamic test = JsonConvert.DeserializeObject(SOPDataList);
                // --- 將資料新增至資料庫
                using (TransactionScope scope = new TransactionScope())
                {

                    //foreach (var item in test)
                    //{
                    //    var org_code = item["ORG_CODE"].ToString().Replace("{", "").Replace("}", "");
                    //    var sop_no = item["SOP_NO"].ToString().Replace("{", "").Replace("}", "");
                    //    var sopdatasearch = sopdata.Where(x => x.orgid == org_code && x.sopno == sop_no).FirstOrDefault();
                    //    // --- 判斷當沒有資料才作新增
                    //    if (sopdatasearch == null)
                    //    {
                    //        tdsopdata = new tdsopdata();
                    //        tdsopdata.orgname = item["ORG_NAME"].ToString().Replace("{", "").Replace("}", "");
                    //        tdsopdata.orgid = org_code;
                    //        tdsopdata.sopname = item["SOP_NAME"].ToString().Replace("{", "").Replace("}", "");
                    //        tdsopdata.sopno = sop_no;
                    //        tdsopdata.url = item["LAWSYS_URL"].ToString().Replace("{", "").Replace("}", "");
                    //        tdsopdata.status = item["STATUS"].ToString().Replace("{", "").Replace("}", "");
                    //        tdsopdata.upddate = DateTime.Parse(item["LAST_UPDATE_TIME"].ToString().Replace("{", "").Replace("}", ""));
                    //        sop_datacontent.tdsopdata.InsertOnSubmit(tdsopdata);
                    //        sop_datacontent.SubmitChanges();
                    //    }
                    //    // --- 判斷當有資料時才作更新
                    //    else
                    //    {
                    //        sopdatasearch.sopname = item["SOP_NAME"].ToString().Replace("{", "").Replace("}", "");
                    //        sopdatasearch.url = item["LAWSYS_URL"].ToString().Replace("{", "").Replace("}", "");
                    //        sopdatasearch.status = item["STATUS"].ToString().Replace("{", "").Replace("}", "");
                    //        sopdatasearch.upddate = DateTime.Parse(item["LAST_UPDATE_TIME"].ToString().Replace("{", "").Replace("}", ""));
                    //        sop_datacontent.SubmitChanges();
                    //    }
                    //    scope.Complete();
                    //}

                    // --- 更新案件對應SOP的SOPID
                    var soplist = sop_datacontent.tccaseInstructiondata.Where(x => x.type == "S" && x.title != "").ToList();

                    foreach (var item2 in soplist)
                    {
                        var sopno = sopdata.Where(x => x.url == item2.description).FirstOrDefault();
                        item2.title = sopno == null ? "" : sopno.sopname;
                        item2.sopno = sopno == null ? "" : sopno.sopno;
                        sop_datacontent.SubmitChanges();
                    }
                    scope.Complete();
                }
            }
        }
    }
}
