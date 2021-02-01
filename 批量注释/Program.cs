using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 批量注释
{
    class Program
    {
        static void Main(string[] args)
        {
            new OracleCode().ConnDB();

            Console.WriteLine("输入1修改字段，输入2修改表");
            var num = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("是否启用百度自动翻译？1:0");
            var isBd = Convert.ToInt32(Console.ReadLine()) == 1;
            if (num == 1)
            {
                SetColumn(isBd);
            }
            if (num == 2)
            {
                SetTale(isBd);
            }

            Console.ReadKey();
        }

        private static void SetTale(bool isBd)
        {
            var list = new OracleCode().GetTables();
            Console.WriteLine($"未注释表记录：{list.Count}条");

            int i = 0;
            foreach (var item in list)
            {
                if (isBd)
                {
                    Console.WriteLine("启用百度翻译");
                    var bd = BaiduFanyi.GetTranslationFromBaiduFanyi(item.TableName);
                    if (string.IsNullOrEmpty(bd.Error_code))
                    {
                        var fy = bd.Trans_result[0].Dst;
                        Console.WriteLine($"翻译结果：{fy}");
                        new OracleCode().UpdateTable(item.TableName, fy);
                        i += 1;
                        Console.WriteLine($"-----------------{i}.修改成功------------------");
                        continue;
                    }
                    Console.WriteLine($"翻译失败,{bd.Error_code}");

                }
                Console.WriteLine($"请输入{item.TableName}的注释");
                var s = Console.ReadLine();

                new OracleCode().UpdateTable(item.TableName, s);
                i += 1;
                Console.WriteLine($"-----------------{i}.修改成功------------------");
            }
        }


        private static void SetColumn(bool isBd)
        {
            var list = new OracleCode().GetColumnInfo();
            Console.WriteLine($"未注释字段：{list.Count}条");
            var goup = list.GroupBy(x => x.ColumnName).ToList();


            foreach (var item in goup)
            {

                var grlist = list.Where(x => x.ColumnName == item.Key).ToList();
                if (isBd)
                {
                    Console.WriteLine("启用百度翻译");
                    var bd = BaiduFanyi.GetTranslationFromBaiduFanyi(item.Key);
                  
                    if (string.IsNullOrEmpty(bd.Error_code))
                    {
                        var fy = bd.Trans_result[0].Dst;
                        Console.WriteLine($"翻译结果：{fy}");
                        Zhushi(item, fy, grlist);
                        continue;
                    }
                    Console.WriteLine($"翻译失败,{bd.Error_code}");
                    continue;

                }
                if (grlist.Count < 5)
                {
                    Zhushi(item, "表字段", grlist);
                    continue;
                }
                Console.WriteLine($"请输入{item.Key}的注释");
                var s = Console.ReadLine();

                Zhushi(item, s, grlist);
            }
        }

        private static void Zhushi(IGrouping<string, ColumnInfo> item, string s, List<ColumnInfo> grlist)
        {
            int i = 0;
            foreach (var a in grlist)
            {
                new OracleCode().UpdateColumn(a.TableName, item.Key, s);
                i += 1;
            }
            Console.WriteLine($"修改{i}条数据");
            Console.WriteLine($"未注释字段：{new OracleCode().GetColumnInfo().Count}条");
        }
    }
}
