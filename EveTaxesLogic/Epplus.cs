using EveSdeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveDataStorage.Contexts;
using EveDataStorage.Models;
using EveSdeModel.Models;
using System.Collections;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
                var headers = new string[] { "Альянс", "Корпорация", "Имя персонажа", "Общий доход с лун", "Общий налог с лун" };
                int numberColumn1 = headers.Length - 1, numberColumn2 = headers.Length;
                sheet.FillRow(1, 1, headers);

                foreach (var character in characterTaxes.OrderBy(x => x.Corporation.AllianceId).ThenBy(x => x.Corporation.Name).ThenBy(x => x.CharacterName))
                {
                    var values = new string[] { (character.Corporation.Alliance != null ? character.Corporation.Alliance.Name : ""),
                        character.Corporation.Name, character.CharacterName, character.TotalIskGain.ToString(), character.TotalIskTax.ToString() };
                    sheet.FillRow(rowNum, 1, values);
                    rowNum++;
                }
                sheet.Cells[2, numberColumn1, rowNum-1, numberColumn2].Style.Numberformat.Format = "#,##0";

                sheet.Cells.Style.Font.Name = "Calibri";
                sheet.Cells.AutoFitColumns();
                package.SaveAs(path);
            }
        }

        private static void FillRow(this ExcelWorksheet sheet, int rowNum, int colNum, IEnumerable<string> values)
        {
            var col = colNum;
            foreach (var value in values)
            {
                sheet.Cells[rowNum, col].Value = value;
                col++;
            }
        }
    }
}
