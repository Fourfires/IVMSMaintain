using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using IVMSMaintain.Helper;
using LinqToDB;
using Models;
namespace IVMSMaintain.Controllers
{
    public class SystemManageController : Controller
    {
        // GET: SystemManage
        public ActionResult GetView()
        {

            return View("SystemManage");
        }
        /// <summary>
        /// 导入所有汇聚信息到本地,并创建ivms_server表格
        /// </summary>
        public string ImportConverge()
        {
            string localConnString = "Server=127.0.0.1;Port=3306;Database=ivmsmaintain;Uid=root;Pwd=12345;charset=utf8;";
            var list = new List<string>();
            list.Add("10.221.24.221");
            IvmsServer center = new IvmsServer()
            {
                Id = 0,
                Description = "中心汇聚",
                SourceIp = "0",
                Address = "10.221.24.221"
            };
            using (var db = new IvmsmaintainDB())
            {
                db.IvmsServer.Delete();
                db.Insert(center);

                using (var centerDb = new OpensipsDB("opensips_center"))
                {
                    var convergeQuery = from p in centerDb.DrGateways
                        where !string.IsNullOrEmpty(p.Address) && p.Description.Contains("汇聚")
                        select new IvmsServer()
                        {
                            SourceIp = center.Address,

                            Description = p.Description,
                            Address = p.Address,
                            Gwid = p.Gwid,
                        };
                    var convergeList = convergeQuery.ToList();
                    list.AddRange(convergeList.Select(p => p.Address));
                    //foreach (var VARIABLE in convergeList)
                    //{
                    //    db.Insert(VARIABLE);
                    //}
                }
                Thread.Sleep(1000);
                
                var dt = ExcuteQuery(list, "opensips", "select * from dr_gateways", "ivms_server");
                MySqlHelper.CreateTable(localConnString, dt);
                MySqlHelper.BulkInsert(localConnString, dt);
                var list2 = dt.Rows.Cast<DataRow>().Where(p => p["description"].ToString().Contains("汇聚"))
                    .Select(p => p["address"].ToString()).ToList();
                var dt2 = ExcuteQuery(list2, "opensips", "select * from dr_gateways", "ivms_server");
                MySqlHelper.CreateTable(localConnString, dt2);
                MySqlHelper.BulkInsert(localConnString, dt2);
                
            }
            return "结束";
        }
        public DataTable ExcuteQuery(List<string> serverList, string dbName, string querySql, string tableName)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;
            bool first = true;
            foreach (var item in serverList)
            {
                try
                {
                    string serverIp = item.ToString();
                    MySqlHelper helper = new MySqlHelper(serverIp, dbName, "root", "kys311");
                    if (!helper.TestConn())
                    {
                        helper = new MySqlHelper(serverIp, dbName, "root", "12345");
                        if (helper.TestConn())
                        {
                            return new DataTable();
                        }
                    }

                    DataTable curDt = helper.ExecuteDataTable(querySql);
                    {
                        if (first)
                        {
                            first = !first;
                            dt = curDt;
                            dt.TableName = tableName;
                            dt.Columns.Add("sourceIp", typeof(string));
                            foreach (DataRow dr in dt.Rows)
                            {
                                dr["sourceIp"] = serverIp;
                            }
                        }
                        else
                            foreach (DataRow dr in curDt.Rows)
                            {
                                List<object> drValuesList = new List<object>(dr.ItemArray);
                                drValuesList.Add(serverIp);
                                dt.Rows.Add(drValuesList.ToArray());
                            }
                    }
                }
                catch { }
            }
            return dt ?? (dt = new DataTable());
        }
        public void FormatAllStation()
        {
            List<TServerManager> result = new List<TServerManager>();
            List<IvmsServer> list = new List<IvmsServer>();
            using (var db = new IvmsmaintainDB("localhost_ivmsmaintain"))
            {
                var query = from p in db.IvmsServer
                            where !string.IsNullOrEmpty(p.Address) && !string.IsNullOrEmpty(p.Description)
                            //where p.Description.Contains(partten)
                            //  join q in db.IvmsServer on p.SourceIp equals q.Address 
                            // from item in convergeGroup.DefaultIfEmpty(new IvmsServer { Description=string.Empty})
                            select p;
                //select new {StationName=p.Description,Converge=p.SourceIp,ServerIp=p.Address};
                list = query.ToList();
            }
            if (list.Count > 0)
            {
                //对list去重
                var newList = list.GroupBy(p => p.Description)
                    //.Select(p => p.First());
                    .SelectMany(p =>
                    {
                        var sameStationList = p.ToList();
                        var filterResult = sameStationList.GroupBy(q => q.SourceIp).Select(q => q.First());
                        return filterResult.ToList();
                    }).ToList();
                foreach (var item in newList)
                {
                    if (string.IsNullOrEmpty(item.Address) || string.IsNullOrEmpty(item.Description))
                        continue;
                    TServerManager isi = new TServerManager();
                    var query2 = from p in newList
                                 where p.Address.Equals(item.SourceIp) && p.Description.Contains("汇聚")
                                 select p;

                    foreach (var converge in query2)
                    {
                        using (var line = new IvmsmaintainDB("localhost_ivmsmaintain"))
                        {
                            var lineQuery = from q in line.IvmsLine
                                            where q.Stations.Contains(item.Description.Trim())
                                            select q;
                            foreach (var lineResult in lineQuery)
                            {
                                if (lineResult.Stations.Split(',').Contains(item.Description.Trim()))
                                {
                                    isi.Line = lineResult.Name;
                                    isi.Path = lineResult.Path;
                                    break;
                                }
                            }
                            isi.ConvergeName = converge.Description;
                        }
                    }
                    isi.StationName = item.Description;
                    isi.Ip = item.Address;
                    isi.ConvergeIp = item.SourceIp;
                    //try
                    //{
                    //    isi.StationNo =
                    //        IvmsResourceManager.GetStationNoByXml(IvmsResourceManager.GetConfigXml(item.Address));
                    //}
                    //catch
                    //{
                    //    isi.StationNo = "未知";

                    //}
                    isi.StationNo = item.PriPrefix;
                    result.Add(isi);
                }
            }
            {
                using (var db = new IvmsmaintainDB("localhost_ivmsmaintain"))
                {
                    foreach (var item in result)
                        db.Insert(item);
                }
            }
        }
    }
}