using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDFTech;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OrderLinc.DTOs;

namespace OrderLinc.NotificationLib
{

    public class ReportService
    {
        DTOSYSConfig _Configuration = new DTOSYSConfig();
        string _reportPath;
        public static bool? _isPepsiCoDistributor;
        static Table headertable;
        static Table table;

        public bool? IsPepsiCoDistributor { get; set; }
        public string PDFImagePath = "";

        public ReportService(string reportPath)
        {
            _reportPath = reportPath;
            PrepareOutputdir(_reportPath);
        }

        private void PrepareOutputdir(string mPath) //checks and create if necessary
        {
            DirectoryInfo dirInfo = null;
            if (!Directory.Exists(mPath))
            {
                dirInfo = Directory.CreateDirectory(mPath);
            }
        }

        public String RenderCheckoutForm(ReportHeader reportHeader, DTOCustomer customer, DTOOrder dtoOrder, DTOOrderLineList detailsDTO, DTOAddress customerAddressDetails ,string OrderSignaturePath)
        {

            _isPepsiCoDistributor = this.IsPepsiCoDistributor;
            try
            {
                if (File.Exists(OrderSignaturePath))
                {

                    PDFDocument.License = "EORIFBSR-2049-198-O004F";

                    PDFCreationOptions options = new PDFCreationOptions();
                    options.SetMargin(50, 50, 50, 130);
                    options.PageSize = PDFPageSize.A4;
                    options.ComplianceStandard = PDFStandards.PDFA1b;

                    // Make this the default filename.
                    string filename = Path.Combine(_reportPath, "Order(" + dtoOrder.OrderNumber + ").pdf");

                    bool isRegularOrder = (dtoOrder.IsRegularOrder != null && dtoOrder.IsRegularOrder == true);

                    if (isRegularOrder == true)
                    {
                        filename = Path.Combine(_reportPath, "Order(" + dtoOrder.OrderNumber + ").pdf");
                    }
                    else if (isRegularOrder == false)
                    {
                        filename = Path.Combine(_reportPath, "Pre-sell Order(" + dtoOrder.OrderNumber + ").pdf");
                    }

                    PDFDocument doc = new PDFDocument(filename, options);

                    
                    // Create the Intended Footer here from an image
                    // If this is for PepsiCo Distributor, then show

                    // Footer - PDF                    

                    if (IsPepsiCoDistributor != null && IsPepsiCoDistributor == true)
                    {
                        try
                        {
                            PDFImage img = new PDFImage(this.PDFImagePath);
                            if (img != null)
                            {
                                doc.PageFooter.AddImage(img);
                            }

                        }
                        catch
                        {
                            // do nothing, just ignore.
                        }
                    }

                    doc.DocumentInfo.Keywords = customer.CustomerName;

                    doc.AutoLaunch = false;
                    PageNumberingRange range = doc.AddNumberingRange(0, 1);
                    range.PageNumberPrefix = "Page ";

                    DrawTable(reportHeader, dtoOrder, customer, detailsDTO, customerAddressDetails);

                    doc.CurrentPage.Body.SetActiveFont("Arial", PDFFontStyles.Regular, 11);
                    doc.CurrentPage.Body.SetTextAlignment(TextAlign.Center);
                    doc.CurrentPage.Body.AddText("ORDER DETAILS");
                    doc.CurrentPage.Body.SetActiveFont("Arial", PDFFontStyles.Regular, 9);
                    doc.CurrentPage.Body.AddText("\r\n");
                    doc.CurrentPage.Body.DrawTable(headertable);
                    Image im = Image.FromFile(OrderSignaturePath);
                    doc.CurrentPage.Body.AddImage(new PDFImage(resizeImage(im, new Size(200, 100))));
                    if (dtoOrder.HoldUntilDate != null)
                    {
                        doc.CurrentPage.Body.AddText("\n");
                        doc.CurrentPage.Body.AddText("Note: Order will not be released until " + String.Format("{0:dd/MM/yyyy}", dtoOrder.HoldUntilDate));
                    }


                    doc.CurrentPage.Body.DrawTable(table);

                    doc.CurrentPage.Body.SetTextAlignment(TextAlign.Left);

                    doc.CurrentPage.Body.AddText("\n\n\n\n\n");
                    doc.CurrentPage.Body.AddText("\n\n\n\n\n");

                    doc.CurrentPage.Body.AddText("\n\n\n");
                    // doc.CurrentPage.Body.AddText("Page Rendered On: " + DateTime.Now.ToString());

                    doc.Save();

                    //if (!doc.Save())
                    //    MessageBox.Show(doc.Error);

                    return filename;

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("OrderLinc", " RenderCheckoutForm - error " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.InnerException.ToString(), EventLogEntryType.Information);

                return "";
            }
        }
        public bool ThumbnailCallback()
        {
            return false;
        }
        public static Bitmap resizeImage(Image imgToResize, Size size)
        {
            try
            {
                Image img2;
                using (Bitmap b = new Bitmap(imgToResize.Width, imgToResize.Height))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.Clear(Color.White);
                        g.DrawImageUnscaled(imgToResize, 0, 0);
                    }

                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                    b.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                    byte[] bytes = memoryStream.ToArray();
                    img2 = Image.FromStream(new System.IO.MemoryStream(bytes));
                    // Now save b as a JPEG like you normally would
                }
                Bitmap bm = new Bitmap(img2, size);
                return bm;
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("OrderLinc", " resizeImage - error " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.InnerException.ToString(), EventLogEntryType.Information);
                return null;
            }


        }


        private static void DrawTable(ReportHeader reportHeader, DTOOrder order, DTOCustomer customer, DTOOrderLineList DetailListDTO, DTOAddress customerAddressDetails)
        {
            System.Globalization.CultureInfo cinfo = System.Globalization.CultureInfo.GetCultureInfo("en-us");

            headertable = new Table(4);
            headertable.style.borderWidth = 0;

            //headertable.column(0).width = 200;
            //headertable.column(1).width = 220;
            //headertable.column(2).width = 220;

            //headertable.column(0).width = 230;
            //headertable.column(1).width = 210;
            //headertable.column(2).width = 230;

            headertable.column(0).width = 190;
            headertable.column(1).width = 220;
            headertable.column(2).width = 50; // Space?
            headertable.column(3).width = 200;

            headertable.style.fontSize = 9;
            headertable.style.fontStyle = TableFontStyle.regular;

            // Order details table
            table = new Table(7); // incremented to allow UOM
            table.style.borderWidth = 0;
            table.column(0).width = 60;
            table.column(1).width = 100;
            table.column(2).width = 100;
            table.column(3).width = 270;
            table.column(4).width = 60;
            table.column(5).width = 50;
            table.column(6).width = 50;

            table.style.fontSize = 9;
            table.style.fontStyle = TableFontStyle.regular;

            table.addRow();
            table.row(table.rowCount - 1).style.borderWidth = 0;

            // Row 1
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.row(headertable.rowCount - 1).style.borderWidth = 0;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).SetValue(reportHeader.SalesOrgName + "  -  " + reportHeader.SalesOrgPhone);

            if (order.PONumber == null || order.PONumber.Trim() == "")
            {
            }
            else
            {
                if ( _isPepsiCoDistributor != null && _isPepsiCoDistributor == true)
                {
                    headertable.cell(headertable.rowCount - 1, 3).style.fontColor = Color.Black;
                    headertable.cell(headertable.rowCount - 1, 3).SetValue("PO No: " + order.PONumber);
                }
            }


            // Row 2
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.row(headertable.rowCount - 1).style.borderWidth = 0;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).SetValue("Order No: " + order.OrderNumber);            
            headertable.cell(headertable.rowCount - 1, 1).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 1).style.textAlign = TextAlignment.right;
            headertable.cell(headertable.rowCount - 1, 1).SetValue("Order Date: " + string.Format("{0:d MMM yyyy}", order.OrderDate));
            headertable.cell(headertable.rowCount - 1, 3).style.fontColor = Color.Black;            
            headertable.cell(headertable.rowCount - 1, 3).SetValue("Sales Rep: " + reportHeader.SalesRepName);

            // Row 3
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.row(headertable.rowCount - 1).style.borderWidth = 0;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).SetValue("Store Number: " + customer.CustomerCode);
            headertable.cell(headertable.rowCount - 1, 1).style.fontColor = Color.Black;
            
            // REGULAR ORDER
            if (order.RequestedReleaseDate != null && order.IsRegularOrder != null)
            {
                if (((DateTime)order.RequestedReleaseDate).Date > order.OrderDate.Date)
                {
                    headertable.cell(headertable.rowCount - 1, 1).style.textAlign = TextAlignment.right;
                    headertable.cell(headertable.rowCount - 1, 1).SetValue("Order Release Date: " + string.Format("{0:d MMM yyyy}", order.RequestedReleaseDate));                    
                }
            }

            // PRE-SELL ORDER
            if (order.IsRegularOrder != null && ((bool)order.IsRegularOrder) == false)
            {
                headertable.cell(headertable.rowCount - 1, 1).style.textAlign = TextAlignment.right;
                headertable.cell(headertable.rowCount - 1, 1).SetValue("Order delivered when stock available");                
            }
            
            headertable.cell(headertable.rowCount - 1, 3).style.fontColor = Color.Black;


            if (_isPepsiCoDistributor != null && _isPepsiCoDistributor == true)
            {
                headertable.cell(headertable.rowCount - 1, 3).SetValue("Provider: " + reportHeader.ProviderWareHouse);
            }
            else
            {
                headertable.cell(headertable.rowCount - 1, 3).SetValue("Provider: " + order.ProviderName);
            }

            

            // Row 4
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).Merge(CellMergeOption.MergeRight, 1);
            headertable.cell(headertable.rowCount - 1, 0).SetValue("Store Details: " + customer.CustomerName);
            headertable.cell(headertable.rowCount - 1, 3).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 3).SetValue("Warehouse: " + reportHeader.ProviderWareHouse);

            // Row 5/6
            // ContactDetails
            if (customerAddressDetails != null)
            {
                headertable.addRow();
                headertable.row(headertable.rowCount - 1).height = 30;
                headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
                headertable.cell(headertable.rowCount - 1, 0).Merge(CellMergeOption.MergeRight, 2);
                headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
                headertable.cell(headertable.rowCount - 1, 0).SetValue("                       " + customerAddressDetails.AddressLine1 + "   " + customerAddressDetails.AddressLine2 + "   " + customerAddressDetails.CitySuburb + "   " + customerAddressDetails.PostalZipCode);
                headertable.row(headertable.rowCount - 1).style.paddingBottom = 0.0d;

                headertable.addRow();
                headertable.row(headertable.rowCount - 1).height = 30;
                headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
                headertable.cell(headertable.rowCount - 1, 0).Merge(CellMergeOption.MergeRight, 2);
                headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
                headertable.cell(headertable.rowCount - 1, 0).SetValue("                       " + customer.StateName);

            }

            // Row 7 
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.row(headertable.rowCount - 1).style.borderWidth = 0;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).SetValue("Store Manager: " + reportHeader.StoreManagerName);

            // Row 8
            headertable.addRow();
            headertable.row(headertable.rowCount - 1).height = 30;
            headertable.row(headertable.rowCount - 1).style.borderWidth = 0;
            headertable.cell(headertable.rowCount - 1, 0).style.fontColor = Color.Black;
            headertable.cell(headertable.rowCount - 1, 0).SetValue("Signature: ");
            
            //Headers Title
            table.addRow();
            table.cell(table.rowCount - 1, 0).SetValue("Line# ");
            table.cell(table.rowCount - 1, 1).SetValue("| Product Code |");
            table.cell(table.rowCount - 1, 2).SetValue("GTIN ");
            table.cell(table.rowCount - 1, 3).SetValue("| Product Description  ");
            table.cell(table.rowCount - 1, 4).SetValue("| Units | ");
            table.cell(table.rowCount - 1, 5).SetValue("UOM  | "); // added to accommodate UOM
            table.cell(table.rowCount - 1, 6).style.textAlign = TextAlignment.center;
            table.cell(table.rowCount - 1, 6).SetValue("Qty");

            table.row(table.rowCount - 1).style.borderBottomWidth = 1;

            table.addRow();

            List<DTOOrderLine> Sorted = DetailListDTO.OrderBy(p => p.LineNum).ToList();

            foreach (DTOOrderLine mDetail in Sorted)
            {

                int lineno = int.Parse(mDetail.LineNum.ToString());

                //decimal TotalPrice = 0.0M;

                table.addRow();
                table.row(table.rowCount - 1).style.borderWidth = 0;

                table.cell(table.rowCount - 1, 0).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 0).SetValue(lineno.ToString());

                table.cell(table.rowCount - 1, 1).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 1).SetValue(mDetail.ProductCode.ToString());

                //table.cell(table.rowCount - 1, 1).style.borderRightWidth = 1;

                table.cell(table.rowCount - 1, 2).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 2).SetValue("" + mDetail.GTINCode);

                //table.cell(table.rowCount - 1, 1).style.borderRightWidth = 1;

                table.cell(table.rowCount - 1, 3).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 3).SetValue(mDetail.ProductName);

                //table.cell(table.rowCount - 1, 2).style.borderRightWidth = 1;

                table.cell(table.rowCount - 1, 4).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 4).SetValue(mDetail.PrimarySKU);

                table.cell(table.rowCount - 1, 5).style.textAlign = TextAlignment.left;
                table.cell(table.rowCount - 1, 5).SetValue(mDetail.UOM.ToString());

                table.cell(table.rowCount - 1, 6).style.textAlign = TextAlignment.center;
                table.cell(table.rowCount - 1, 6).SetValue(mDetail.OrderQty.ToString());
            }

            //table.addRow();
            ////table.row(table.rowCount - 1).style.borderWidth = 0;

            //table.cell(table.rowCount - 1, 0).Merge(CellMergeOption.MergeRight, 4);
            //table.cell(table.rowCount - 1, 0).style.textAlign = TextAlignment.left;
            //table.cell(table.rowCount - 1, 0).SetValue("Page Rendered On: " + DateTime.Now.ToString());

            //table.cell(table.rowCount - 1, 3).style.textAlign = TextAlignment.right;
            //table.cell(table.rowCount - 1, 3).SetValue(TotalCustomer.ToString(moneyformat, cinfo));

        }





    } //class
}
