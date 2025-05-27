using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel;
using EveSdeModel.Models;
using EveTaxesLogic.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic
{
    public static class Epplus
    {
        public static void Export(string path, IEnumerable<CharacterTax> characterTaxes)
        {
            ExcelPackage.License.SetNonCommercialPersonal("EveApp");
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("MySheet");

                var rowNum = 2;
                var headers = new string[] { "Альянс", "Корпорация", "Имя персонажа", "Общий доход", "Общий налог", "Общий доход с лун", "Общий налог с лун", "Крабский доход", "Крабский налог"};
                int numberColumn1 = headers.Length - 1, numberColumn2 = headers.Length;
                sheet.FillRow(1, 1, headers);

                foreach (var character in characterTaxes.OrderBy(x => x.Corporation.AllianceId).ThenBy(x => x.Corporation.Name).ThenBy(x => x.CharacterName))
                {
                    var values = new string[] { (character.Corporation.Alliance != null ? character.Corporation.Alliance.Name : ""),
                        character.Corporation.Name, character.CharacterName, character.TotalIskGain.ToString(), character.TotalIskTax.ToString(), 
                        character.TotalIskGain_MoonMining.ToString(), character.TotalIskTax_MoonMining.ToString(),
                        character.TotalIskGain_Ratting.ToString(), character.TotalIskTax_Ratting.ToString(),
                    };
                    sheet.FillRow(rowNum, 1, values);
                    rowNum++;
                }
                sheet.Cells[2, numberColumn1, rowNum - 1, numberColumn2].Style.Numberformat.Format = "#,##0";

                sheet.Cells.Style.Font.Name = "Calibri";
                sheet.Cells.AutoFitColumns();
                package.SaveAs(path);
            }
        }

        public static void Export(string path, IEnumerable<CorporationTax> corporationTaxes, int[] allianceIds)
        {
            ExcelPackage.License.SetNonCommercialPersonal("EveApp");
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("MySheet");

                var startCol = 1;
                var rowNum = 1;
                var headers = new string[] { "Альянс", "Корпорация", "Имя пользователя", "Имя персонажа", "Общий доход", "Общий налог", "Общий доход с лун", "Общий налог с лун", "Общий доход с минералов", "Общий налог с минералов", "Крабский доход", "Крабский налог" };
                int numberColumn1 = headers.Length - 1, numberColumn2 = headers.Length;
                sheet.FillRow(rowNum, startCol, headers);
                rowNum++;

                var notNull = corporationTaxes.Where(x => x.AllianceId.HasValue && allianceIds.Contains(x.AllianceId.Value)).ToList();
                var dictAlliances = notNull.GroupBy(x => x.AllianceId.Value).ToDictionary(x => x.Key, x => x.ToList());

                foreach (var alliance in dictAlliances.OrderBy(x => x.Key))
                {
                    var allianceInfo = alliance.Value.First()?.Alliance;
                    string allianceName = "no alliance";
                    if (allianceInfo != null)
                    {
                        allianceName = allianceInfo.Name;
                    }

                    var summAllianceGain = alliance.Value.Sum(x => x.TotalIskGain);
                    var summAllianceTaxes = alliance.Value.Sum(x => x.TotalIskTax);

                    var summAllianceGain_MoonMining = alliance.Value.Sum(x => x.TotalIskGain_MoonMining);
                    var summAllianceTaxes_MoonMining = alliance.Value.Sum(x => x.TotalIskTax_MoonMining);

                    var TotalIskGain_MineralMining = alliance.Value.Sum(x => x.TotalIskGain_MineralMining);
                    var TotalIskTax_MineralMining = alliance.Value.Sum(x => x.TotalIskTax_MineralMining);

                    var summAllianceGain_Ratting = alliance.Value.Sum(x => x.TotalIskGain_Ratting);
                    var summAllianceTaxes_Ratting = alliance.Value.Sum(x => x.TotalIskTax_Ratting);

                    var allianceHeader = new object[] { allianceName, "", "", "", summAllianceGain, summAllianceTaxes, summAllianceGain_MoonMining, summAllianceTaxes_MoonMining, TotalIskGain_MineralMining, TotalIskTax_MineralMining, summAllianceGain_Ratting, summAllianceTaxes_Ratting };
                    sheet.FillRow(rowNum, startCol, allianceHeader);
                    rowNum++;

                    foreach (var corporation in alliance.Value.OrderBy(x => x.CorporationId))
                    {
                        var corporationHeader = new object[] { "", corporation.CorporationName, "", "", corporation.TotalIskGain, corporation.TotalIskTax, corporation.TotalIskGain_MoonMining, corporation.TotalIskTax_MoonMining, corporation.TotalIskGain_MineralMining, corporation.TotalIskTax_MineralMining, corporation.TotalIskGain_Ratting, corporation.TotalIskTax_Ratting };
                        sheet.FillRow(rowNum, startCol, corporationHeader);
                        rowNum++;

                        foreach (var user in corporation.UserTaxes.OrderBy(x => x.MainCharacterId))
                        {
                            //  если персонажей несколько
                            if (user.CharacterTaxes.Count > 1)
                            {
                                var userHeader = new object[] { "", "", user.Name, "", user.TotalIskGain, user.TotalIskTax, user.TotalIskGain_MoonMining, user.TotalIskTax_MoonMining, user.TotalIskGain_MineralMining, user.TotalIskTax_MineralMining, user.TotalIskGain_Ratting, user.TotalIskTax_Ratting };
                                sheet.FillRow(rowNum, startCol, userHeader);
                                rowNum++;

                                foreach (var characterTax in user.CharacterTaxes.OrderBy(x => x.CharacterId))
                                {
                                    var characterTaxRow = new object[] { "", "", "", characterTax.CharacterName, characterTax.TotalIskGain, characterTax.TotalIskTax, characterTax.TotalIskGain_MoonMining, characterTax.TotalIskTax_MoonMining, characterTax.TotalIskGain_MineralMining, characterTax.TotalIskTax_MineralMining, characterTax.TotalIskGain_Ratting, characterTax.TotalIskTax_Ratting };
                                    sheet.FillRow(rowNum, startCol, characterTaxRow);
                                    rowNum++;
                                }
                            }
                            else
                            {
                                var _char = user.CharacterTaxes.FirstOrDefault();
                                if (_char != null)
                                {
                                    var userHeader = new object[] { "", "", user.Name, _char.CharacterName, user.TotalIskGain, user.TotalIskTax, user.TotalIskGain_MoonMining, user.TotalIskTax_MoonMining, user.TotalIskGain_MineralMining, user.TotalIskTax_MineralMining, user.TotalIskGain_Ratting, user.TotalIskTax_Ratting };
                                    sheet.FillRow(rowNum, startCol, userHeader);
                                    rowNum++;
                                }
                            }
                        }
                        rowNum++;
                        rowNum++;
                    }
                    rowNum++;
                    rowNum++;
                }

                sheet.Cells.Style.Font.Name = "Calibri";
                sheet.Cells.AutoFitColumns();
                package.SaveAs(path);
            }
        }

        private static void FillRow(this ExcelWorksheet sheet, int rowNum, int colNum, IEnumerable<object> values)
        {
            var col = colNum;
            foreach (var value in values)
            {
                sheet.Cells[rowNum, col].Value = value;
                if (value.GetType() == typeof(int) || value.GetType() == typeof(long))
                {
                    sheet.Cells[rowNum, col].Style.Numberformat.Format = "#,##0";
                }
                col++;
            }
        }
    }
}
