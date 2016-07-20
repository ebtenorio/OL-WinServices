using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Diagnostics;
using PL.PersistenceServices;
using PL.PersistenceServices.DTOS;
using PL.PersistenceServices.Enumerations;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using OrderLinc.DTOs;
using OrderLinc;
using OrderLinc.Services;
using OrderLinc.IDataContracts;

namespace OrderLinc.EDIFileProcessor

{
    public class OrderTOOWriter
    {
        Timer OrderAppTimer = new Timer();

        string mServerName;
        string mDatabasename;
        string mUsername;
        string mPassword;

        OrderLincServices _orderLincService;
        IOrderService _orderService;
        ICustomerService _customerService;
        IConfigurationService _configurationService;
        IProviderService _providerService;
        IAddressService _addressService;
        ILogService _logService;

        string unsentFolder = string.Empty;

        public OrderTOOWriter(OrderLincServices orderLincService)
        {
            try
            {

                //mServerName = serverName;
                //mDatabasename = databaseName;
                //mUsername = userName;
                //mPassword = password;

                _orderLincService = orderLincService;

                _orderService = _orderLincService.OrderService;
                _customerService = _orderLincService.CustomerService;
                _configurationService = _orderLincService.ConfigurationService;

                _providerService = _orderLincService.ProviderService;
                _addressService = _orderLincService.AddressService;
                _logService = _orderLincService.LogService;
            }
            catch (Exception e)
            {
                this.WriteEvent("OrderAppParserLib()", e);
                _logService.LogSave("OrderTOOWriter", e.Message, 0);
                throw new Exception("OrderAppParserLib(): " + e.Message + Environment.NewLine + e.InnerException.ToString());
            }

        }

        public void Start()
        {
            try
            {

              //  EventLog.WriteEntry("OrderAppParserService", "StartParserService Invoked");
                DTOSYSConfig interval = _configurationService.SYSConfigListByKey("ParserInterval");


                OrderAppTimer.Interval = Int32.Parse(interval.ConfigValue) * 1000;
                unsentFolder = _configurationService.SYSConfigListByKey("Unsent").ConfigValue;

                //EventLog.WriteEntry("OrderAppParserService", "Server: " + mServerName + Environment.NewLine +
                //             "Database: " + mDatabasename + Environment.NewLine +
                //             "Username: " + mUsername + Environment.NewLine +
                //             "Password: " + mPassword + Environment.NewLine +
                //             "Interval: " + OrderAppTimer.Interval, EventLogEntryType.Information);

                OrderAppTimer.Elapsed += new ElapsedEventHandler(OnAppTimerElapsed);
                OrderAppTimer.Enabled = true;
                OrderAppTimer.Start();
            }
            catch (Exception e)
            {
                this.WriteEvent("StartParserService", e);
                _logService.LogSave("StartParserService", e.Message, 0);
                throw new Exception("OrderAppParserLib.StartService(): " + e.Message + Environment.NewLine + e.InnerException.ToString());
            }

        }

        public void Stop()
        {
            try
            {
                EventLog.WriteEntry("OrderAppParserService", "StopParserService Invoked");

                OrderAppTimer.Enabled = false;
            }
            catch (Exception e)
            {
                this.WriteEvent("StopParserService", e);
                _logService.LogSave("StopParserService", e.Message, 0);
                throw new Exception("OrderAppParserLib.StopService(): " + e.Message + Environment.NewLine + e.InnerException.ToString());
            }
        }

        private void OnAppTimerElapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                OrderAppTimer.Stop();

                this.ProcessTurnInOrders();

                OrderAppTimer.Start();
            }
            catch (Exception ex)
            {
                this.WriteEvent("OnAppTimerElapsed", ex);
                _logService.LogSave("StopParOnAppTimerElapsedserService", ex.Message, 0);
                throw new Exception("OrderAppParserLib.OnAppTimerElapsed(): " + ex.Message + Environment.NewLine + ex.InnerException.ToString());

            }
            finally {

                OrderAppTimer.Start();
            }
        }


        public Boolean ProcessTurnInOrders()
        {
            try
            {

                List<DTOOrder> OrderList = new List<DTOOrder>();
                List<DTOOrderLine> OrderLineList = new List<DTOOrderLine>();
                string DELIMITER = "|";
                string LINETERMINATOR = "~";
                int OrderCounter = 0;

                OrderList = _orderService.OrderListByOrderStatus(false, false);

                foreach (DTOOrder Order in OrderList)
                {

                    DTOProvider provider = _providerService.ProviderListByID(Order.ProviderID);
                    DTOSalesOrg salesOrg = _customerService.SalesOrgListByID(Order.SalesOrgID);
                    DTOProviderWarehouse providerWarehouse = _providerService.ProviderWarehouseListByID(Order.ProviderWarehouseID);
                    DTOCustomer customer = _customerService.CustomerListByID(Order.ProviderID, Order.CustomerID);
                    DTOAddress billToAddress = _addressService.AddressListByID(customer.BillToAddressID);
                    DTOAddress shipToAddress = _addressService.AddressListByID(customer.ShipToAddressID);
                    
                    if (provider == null)
                        provider = new DTOProvider();

                    if (salesOrg == null)
                        salesOrg = new DTOSalesOrg();

                    if (providerWarehouse == null)
                        providerWarehouse = new DTOProviderWarehouse();

                    if (customer == null)
                        customer = new DTOCustomer();

                    if (billToAddress == null)
                        billToAddress = new DTOAddress();

                    if (shipToAddress == null)
                        shipToAddress = new DTOAddress();

                    string providerCode = provider == null ? string.Empty : provider.ProviderCode;
                    string salesOrgCode = salesOrg == null ? string.Empty : salesOrg.SalesOrgCode;
                    string salesOrgName = salesOrg == null ? string.Empty : salesOrg.SalesOrgName;
                    string providerWarehouseCode = providerWarehouse == null ? string.Empty : providerWarehouse.ProviderWarehouseCode;
                    string customerCode = customer == null ? string.Empty : customer.CustomerCode;
                    string customerName = customer == null ? string.Empty : customer.CustomerName;

                    //EventLog.WriteEntry("OrderAppParserService", "Orders processed.", EventLogEntryType.Information);
                    StringBuilder TOOrdersStringBuilder = new StringBuilder();
                    OrderLineList =_orderService.OrderLineListByOrderID(Order.OrderID);

                    if(providerCode == "IGA")
                    {
                        customerCode = this.AddLeadingZeros(customerCode.Trim(), 8, true); // Customer Charge-To Wholesaler Account from 17 characters
                    }
                    else
                    {
                        customerCode = this.AddLeadingZeros(customerCode.Trim(), 5, true); // Customer Charge-To Wholesaler Account from 17 characters
                    }

                    // HEADER
                    TOOrdersStringBuilder.Append("HEADER" + DELIMITER); // Record Identifier
                    TOOrdersStringBuilder.Append("TRNOVR" + DELIMITER); // Transaction Type
                    TOOrdersStringBuilder.Append(this.TruncateString(Order.OrderNumber.ToString(), 35) + DELIMITER); // OrderID - Change to OrderNumber - Ringo Ray Piedraverde : 11-3-2014
                    TOOrdersStringBuilder.Append("P" + DELIMITER); // Test-Prod Flag

                    string _SalesOrgCode = this.TruncateString(salesOrgCode, 35);
                    string _ProviderCode = this.TruncateString(providerCode, 35);

                    //if (provider.IsPepsiDistributor != null)
                    //{
                    //    if (provider.IsPepsiDistributor == true)
                    //    {
                    //        _ProviderCode = providerWarehouseCode;
                    //    }
                    //}

                    TOOrdersStringBuilder.Append(_SalesOrgCode + DELIMITER); // Sales Code - Sender 
                    TOOrdersStringBuilder.Append(_ProviderCode + LINETERMINATOR + Environment.NewLine); // Provider Code - Receiver

                    // ORDER HEADER
                    TOOrdersStringBuilder.Append("ORDHDR" + DELIMITER); // Record Identifier
                    TOOrdersStringBuilder.Append(this.TruncateString("", 3) + DELIMITER); // Wholesaler Banner Group
                    TOOrdersStringBuilder.Append(this.TruncateString(providerCode, 17) + DELIMITER); // Wholesaler Identifier
                    TOOrdersStringBuilder.Append(this.TruncateString(providerWarehouseCode, 17) + DELIMITER); // Wholesaler Distribution Centre
                    TOOrdersStringBuilder.Append(this.TruncateString(salesOrgCode, 17) + DELIMITER); // Supplier Code
                    TOOrdersStringBuilder.Append(this.TruncateString(salesOrgName, 35) + DELIMITER);// Supplier Name
                    TOOrdersStringBuilder.Append(this.TruncateString("", 35) + DELIMITER); // Supplier Contact Name - ON HOLD FOR THE MEANTIME
                    TOOrdersStringBuilder.Append(customerCode + DELIMITER); // Customer Charge-To Wholesaler Account from 17 characters
                    TOOrdersStringBuilder.Append(this.TruncateString(this.Remove_tab_newLine(customerName), 35) + DELIMITER); // Customer Charge-To Name & Addr Line 1 | 2-10-14 add remove \t \r \n funtion
                    TOOrdersStringBuilder.Append(this.TruncateString(billToAddress.AddressLine1, 35) + DELIMITER); // Customer Charge-To Name & Addr Line 2
                    TOOrdersStringBuilder.Append(this.TruncateString(billToAddress.AddressLine2, 35) + DELIMITER); // Customer Charge-To Name & Addr Line 3
                    TOOrdersStringBuilder.Append(this.TruncateString(billToAddress.CitySuburb, 35) + DELIMITER);   // Customer Charge-To Name & Addr Line 4
                    TOOrdersStringBuilder.Append(this.TruncateString(billToAddress.PostalZipCode + billToAddress.StateCode, 35) + DELIMITER); // Customer Charge-To Name & Addr Line 5
                    TOOrdersStringBuilder.Append(customerCode + DELIMITER); // Customer Deliver-To Wholesaler Account
                    TOOrdersStringBuilder.Append(this.TruncateString(this.Remove_tab_newLine(customerName), 35) + DELIMITER); // Customer Deliver-To Name & Addr Line 1 | 2-10-14 add remove \t \r \n funtion
                    TOOrdersStringBuilder.Append(this.TruncateString(shipToAddress.AddressLine1, 35) + DELIMITER); // Customer Deliver-To Name & Addr Line 2
                    TOOrdersStringBuilder.Append(this.TruncateString(shipToAddress.AddressLine2, 35) + DELIMITER); // Customer Deliver-To Name & Addr Line 3
                    TOOrdersStringBuilder.Append(this.TruncateString(shipToAddress.CitySuburb, 35) + DELIMITER); // Customer Deliver-To Name & Addr Line 4
                    TOOrdersStringBuilder.Append(this.TruncateString(shipToAddress.PostalZipCode + shipToAddress.StateCode, 35) + DELIMITER); // Customer Deliver-To Name & Addr Line 5
                    //TOOrdersStringBuilder.Append(this.TruncateString(Order.OrderNumber, 17) + DELIMITER); // Order Number (NOTE: delimited to 10 chars) // ORIGINAL LINE OF CODE
                    TOOrdersStringBuilder.Append(this.TruncateString(Order.OrderNumber.ToString(), 17) + DELIMITER); // Order Number (NOTE: delimited to 10 chars) Change to OrderNumber - Ringo Ray Piedraverde : 11-3-2014
                    TOOrdersStringBuilder.Append(Order.OrderDate.ToString("yyyyMMdd") + DELIMITER); // Order Date
                    TOOrdersStringBuilder.Append("" + DELIMITER); // Split Order Flag - NOT USED FOR THE MEANTIME
                    TOOrdersStringBuilder.Append("" + DELIMITER); // Charge Only Flag - NOT USED FOR THE MEANTIME

                    // DELIVERY DATE NOW IS DEFAULTED TO THE RELEASE DATE.
                    TOOrdersStringBuilder.Append(Order.ReleaseDate.Value.ToString("yyyyMMdd") + DELIMITER);
                    TOOrdersStringBuilder.Append("" + DELIMITER); // Forward Charge (Invoice) Date - NOT USED FOR THE MEANTIME
                    TOOrdersStringBuilder.Append("" + DELIMITER); // Conformation Flag - NOT USED FOR THE MEANTIME
                    TOOrdersStringBuilder.Append(this.AddLeadingZeros(OrderLineList.Count.ToString(), 3) + LINETERMINATOR + Environment.NewLine); // Number Order Item

                    // ORDERLINE
                    OrderCounter = 0;

                    foreach (DTOOrderLine Orderline in OrderLineList)
                    {
                        OrderCounter++;
                        TOOrdersStringBuilder.Append("LINITM" + DELIMITER); // Record Identifier
                        TOOrdersStringBuilder.Append(this.AddLeadingZeros(OrderCounter.ToString(), 3) + DELIMITER); // Line Item Number
                        
                        // Suggestion:
                        if (salesOrg.UseGTINExport== true)
                        {
                            // removed the padding of zeroes to 14. GTINs
                            TOOrdersStringBuilder.Append(Orderline.GTINCode + DELIMITER); // Product Code EAN - From 17 characters 
                        }
                        else
                        {
                            TOOrdersStringBuilder.Append(this.AddLeadingZeros(Orderline.ProductCode, 14) + DELIMITER); // Product Code EAN  - 2-10-14 Ringo Ray updated no of chars from 17 to 14 as requested.                                                                                          
                        }

                        // TOOrdersStringBuilder.Append(this.TruncateString(_OrderAppLib.OrderService.GetProductCodeSupplier(Orderline.ProductID), 17) + DELIMITER); // Product Code Supplier
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Product Code Supplier
                        TOOrdersStringBuilder.Append(this.TruncateString(this.Remove_tab_newLine(Orderline.ProductName), 35) + DELIMITER); // Product Description 1 | 2-10-14 add remove \t \r \n funtion
                        TOOrdersStringBuilder.Append(this.TruncateString("", 35) + DELIMITER); // Product Description 2 - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append(this.AddLeadingZeros(Orderline.OrderQty.ToString(), 5) + DELIMITER); // Quantity Ordered
                        
                        // Get the ProductID, then get UOMID, then get UOM
                        //string _UOM = _orderLincService.CatalogService.ProductUOMListByID(
                        //    (int)_orderLincService.CatalogService.ProductListByID(Order.ProviderID, (long)Orderline.ProductID).UOMID
                        //    ).ProductUOM;

                        // February 26, 2016: Instead of getting the default one, the default will configured at app, and will directly use the OrderLine.UOM here, from the OrderLIneList Array
                        string _UOM = Orderline.UOM;

                        if ((_UOM.Trim() == "") || (_UOM == "NA"))
                        {
                            _UOM = _orderLincService.CatalogService.ProductUOMListByID(
                           (int)_orderLincService.CatalogService.ProductListByID(Order.ProviderID, (long)Orderline.ProductID).UOMID
                           ).ProductUOM;
                        }

                        TOOrdersStringBuilder.Append(this.TruncateString(_UOM, 3) + DELIMITER); // Unit of Measure
                        
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Delivery Date - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append("" + DELIMITER); // List Price - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Recommended Retail Price - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Net into Store Price - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append( string.Format("{0:F}", Orderline.Discount) + DELIMITER); // Overriding Discount %
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Tax % - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append("" + DELIMITER); // Tax $ Amount - NOT USED FOR THE MEANTIME
                        TOOrdersStringBuilder.Append("00000" + LINETERMINATOR + Environment.NewLine); // Bonus Quantity - NOT USED FOR THE MEANTIME
                    }

                    // TRAILER
                    TOOrdersStringBuilder.Append("END" + DELIMITER); // Record Identifier
                    TOOrdersStringBuilder.Append("TRNOVR" + DELIMITER); // Transaction Type
                    TOOrdersStringBuilder.Append("P" + DELIMITER); // Test_Prod Flag - get this from config sys table
                    TOOrdersStringBuilder.Append(salesOrgCode + DELIMITER); // Sender Code
                    TOOrdersStringBuilder.Append(providerCode + LINETERMINATOR); // Receiver Code

                    // UNSENT FOLDER
                    // string FileName = _SalesOrgCode.ToUpper() + "_" + _ProviderCode.ToUpper() + "_" + Order.OrderNumber.ToString() + ".TXT"; // Original Filename Convention
                    //string FileName = _SalesOrgCode.ToUpper() + "_" + _ProviderCode.ToUpper() + "_" + Order.OrderID.ToString() + ".TXT"; 
                    string FileName = _SalesOrgCode.ToUpper() + "_" + _ProviderCode.ToUpper() + "_" + Order.OrderNumber + ".TXT"; 

                    if (!Directory.Exists(unsentFolder))
                    {
                        Directory.CreateDirectory(unsentFolder);
                    }

                    StreamWriter TOOStreamWriter = new StreamWriter(unsentFolder + @"\" + FileName);

                    TOOStreamWriter.WriteLine(TOOrdersStringBuilder.ToString());

                    TOOStreamWriter.Close();

                    // UPDATE ORDER STATUSES
                    Order.IsSent = true;
                    Order.SYSOrderStatusID = 103;// SNT
                    _orderService.OrderUpdateStatus(Order);
                }
            }
            catch (Exception ex)
            {
                this.WriteEvent("ProcessTurnInOrders()", ex);
                _logService.LogSave("ProcessTurnInOrders", ex.Message, 0);
                throw new Exception("OrderAppParserLib.ProcessTurnInOrders(): " + ex.Message + Environment.NewLine + ex.InnerException.ToString());
            }

            return true;
        }

        private string AddLeadingZeros(string number, int NumberOfMaximumDigits)
        {
            number = number.Replace(" ", "").Trim(); // Ringo update 2-4-14 - Remove Space
            return (new string('0', NumberOfMaximumDigits - number.Length) + number);
        }

        private string AddLeadingZeros(string number, int NumberOfMaximumDigits, bool isRetainValue)
        {
            number = number.Replace(" ", "").Trim();  // Ringo update 2-4-14 - Remove Space

            if ((isRetainValue) == true && (NumberOfMaximumDigits < number.Length))
            {
                return number;
            }

            return (new string('0', NumberOfMaximumDigits - number.Length) + number);
        }

        private string TruncateString(string StringValue, int Length)
        {
            if (StringValue == null)
                StringValue = "";
            if (StringValue.Length > Length) return StringValue.Substring(0, Length);

            return StringValue;
        }

        private string Remove_tab_newLine(string StringValue)
        { // 2-10-14 Added by Ringo Ray Piedraverde - Remove newline and tabs

            StringValue = StringValue.Trim();

            string newStringValue = Regex.Replace(StringValue, @"[^\u0020-\u007F]", " ");
            return newStringValue;
        }
        private Boolean WriteEvent(String MethodName, Exception exception)
        {
            
            EventLog.WriteEntry("OrderAppParserService", "Method Name:" + MethodName + Environment.NewLine +
                                                         "Error Message: " + exception.Message + Environment.NewLine);
            return true;
        }
    }
}
