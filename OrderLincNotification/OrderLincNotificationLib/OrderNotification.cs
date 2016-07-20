using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using OrderLinc.DTOs;
using OrderLinc;
using OrderLinc.IDataContracts;
using PTI;

namespace OrderLinc.NotificationLib
{
    public class OrderNotification
    {
        //public static OrderAppLib.OrderAppLib mOrderApp;
        private System.Timers.Timer Timer1 = new System.Timers.Timer();
        static Boolean IsBusy = false;
        OrderLincServices _orderLincService;
        IOrderService _orderService;
        ICustomerService _customerService;
        IConfigurationService _configService;
        IAccountService _accountService;
        ILogService _logService;
        IProviderService _providerService;
        ReportService _reportService;
        IAddressService _customerAddress;

        EmailSVC _emailService;

        string MailHost = "";
        string MailSender = "";
        int MailPort = 0;
        string MailUserName = "";
        string MailPassword = "";
        string MailCC = "";
        string MailUseDefaultCredential = "";
        bool mMailUseDefaultCredential = false;
        string EmailFooter = "";

        public OrderNotification(string mServerName, string mDatabaseName, string mAuthenticationType, string mUserName, string mPassword)
        {
            try
            {
                _orderLincService = new OrderLincServices(mServerName, mDatabaseName, mAuthenticationType == "0" ? true : false, mUserName, mPassword);
                _orderService = _orderLincService.OrderService;
                _customerService = _orderLincService.CustomerService;
                _customerAddress = _orderLincService.AddressService;
                _configService = _orderLincService.ConfigurationService;
                _accountService = _orderLincService.AccountService;
                _logService = _orderLincService.LogService;
                _providerService = _orderLincService.ProviderService;
                _reportService = new ReportService(Path.Combine(Path.GetTempPath(), "OrderLinc"));
                
                InitEmailService();
                LogMe("OrderNotification Services has been succesfully initialized.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {

                LogMe("Exception OrderNotification Service Initialization " + ex.Message, EventLogEntryType.Error);
            }

        }

        private void InitEmailService()
        {
            try
            {

                DTOSYSConfig mConfig = new DTOSYSConfig();

                DTOSYSConfigList mDTOSYSConfigList = _configService.SYSConfigList();

                PDFImageFooter = mDTOSYSConfigList.Where(p => p.ConfigKey.ToLower() == "pdfimagefooter").FirstOrDefault();
                ServiceInterval = mDTOSYSConfigList.Where(p => p.ConfigKey.ToLower() == "notifyinterval").FirstOrDefault();

                _reportService.PDFImagePath = PDFImageFooter.ConfigValue;

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailHost" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailHost = mConfig.ConfigValue.ToString();
                }
                else
                {

                    LogMe("Cannot find MailHost configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailSender" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailSender = mConfig.ConfigValue.ToString();
                }
                else
                {
                    LogMe("Cannot find MailSender configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailPort" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailPort = int.Parse(mConfig.ConfigValue.ToString());
                }
                else
                {
                    LogMe("Cannot find MailPort configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }



                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailUserName" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailUserName = mConfig.ConfigValue.ToString();
                }
                else
                {

                    LogMe("Cannot find MailUserName configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailPassword" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailPassword = mConfig.ConfigValue.ToString();
                }
                else
                {

                    LogMe("Cannot find MailPassword configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailCC" select sysconfig).FirstOrDefault();

                if (mConfig != null)
                {
                    MailCC = mConfig.ConfigValue.ToString();
                }
                else
                {

                    LogMe("Cannot find MailCC in the configuration table.", EventLogEntryType.Error);

                    return;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailUseDefaultCredential" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailUseDefaultCredential = mConfig.ConfigValue.ToString();


                    if (MailUseDefaultCredential == "0")
                        mMailUseDefaultCredential = false;
                    else
                        mMailUseDefaultCredential = true;

                }
                else
                {

                    LogMe("Cannot find MailUseDefaultCredential configKey in the configuration table.", EventLogEntryType.Error);

                    return;
                }



                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "EmailFooter" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    EmailFooter = mConfig.ConfigValue.ToString();
                }
                else
                {

                    LogMe("Cannot find MailPort configKey in the configuration table.", EventLogEntryType.Error);
                    return;
                }


                //MailHost = "stg.orderlinc.com";
                //MailSender = "notifications@stg.orderlinc.com";
                //MailPort = 25;
                //MailUserName = "notifications@stg.orderlinc.com";
                //MailPassword = "4solutions";
                //mMailUseDefaultCredential = false;
                //MailCC = "jp.sacay@paperlesstrail.net";
                //mCustomer.Email = "rr.piedraverde@paperlesstrail.net";

                _emailService = new PTI.EmailSVC(MailHost.Trim(), MailPort, mMailUseDefaultCredential, MailUserName.Trim(), MailPassword.Trim(), MailSender.Trim());

                LogMe("Mail Server has been successfully setup", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {

                LogMe("OrderNotification - InitEmailService  - Error " + ex.Message, EventLogEntryType.Error);
            }
        }

        public void LogMe(string message, EventLogEntryType mtype)
        {
            try
            {
                _logService.LogSave("OrderNotification", message, 0);

                EventLog.WriteEntry("OrderLinc", message, mtype);
            }
            catch
            {

            }
        }
        /// <summary>
        /// Start Service
        /// </summary>
        /// <param name="Time"></param>

        public void StartService(double Time)
        {
            try
            {

                Timer1.Interval = Time;


                Timer1.Enabled = true;
                this.Timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.Timer1_Tick);

                LogMe("Service has been succesfully started.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("OrderLinc", e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine + e.InnerException.ToString(), EventLogEntryType.Error);
                _logService.LogSave("OrderNotification", "StartService  - Error " + e.Message, 0);


            }

        }
        /// <summary>
        /// Stop Service
        /// </summary>
        public void StopService()
        {
            try
            {

                Timer1.Enabled = false;

            }
            catch
            {


            }

        }


        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Timer1.Stop();

                if (IsBusy == false)
                {

                    //EventLog.WriteEntry("OrderLinc", "Is busy = " + IsBusy, EventLogEntryType.Information);

                    IsBusy = true;
                    DTOMessageInboundList mMessageInboundList = new DTOMessageInboundList();

                    mMessageInboundList = _orderService.MessageInboundListBySentFlag(false, false);

                    //EventLog.WriteEntry("OrderLinc", "Message Inbound Count: " + mMessageInboundList.Count, EventLogEntryType.Information);
                    //  LogMe("Message Inbound Count: " + mMessageInboundList.Count, EventLogEntryType.Information);
                    foreach (DTOMessageInbound mMsgInbound in mMessageInboundList)
                    {
                        bool isSent = ProcessOrderNotification(mMsgInbound);
                        if (isSent == true)
                        {
                            UpdateMessageInboundSuccess(mMsgInbound);
                        }
                    }
                    IsBusy = false;
                }


            }
            catch (Exception ex)
            {
                IsBusy = false;
                LogMe("Timer1_Tick Error ." + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.InnerException, EventLogEntryType.Error);
                EventLog.WriteEntry("OrderLinc", ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.InnerException, EventLogEntryType.Error);


            }
            finally
            {
                Timer1.Start();
            }

        }

        /// <summary>
        /// Email Notification - Create PDF with order Information then attached to email
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>

        private string CreateEmail(DTOContact customerContact, string customerName)
        {
            StringBuilder mBody = new StringBuilder();

            mBody.Append("Dear  " + customerContact.FirstName + ",").AppendLine();
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("         Your order has been successfully confirmed, for your order details please see the attached file.");
            mBody.AppendLine("");
            mBody.AppendLine("         Customer : " + customerName);
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("OrderLinc");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine(EmailFooter);

            return mBody.ToString();
        }

        private string CreateEmailBodyForDistributor(string distributorName, string customerName, bool IsRegularOrder)
        {
            StringBuilder mBody = new StringBuilder();

            mBody.Append("Dear  " + distributorName + ",").AppendLine();
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("         Your order has been successfully confirmed, for your order details please see the attached file.");
            mBody.AppendLine("");
            mBody.AppendLine("         Customer : " + customerName);
            mBody.AppendLine("");

            if (!IsRegularOrder)
            {
                mBody.AppendLine("         Order Type : Pre-sell");
            }

            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine("OrderLinc");
            mBody.AppendLine("");
            mBody.AppendLine("");
            mBody.AppendLine(EmailFooter);

            return mBody.ToString();
        }


        public Boolean ProcessOrderNotification(DTOMessageInbound msgInbound)
        {
            long orderID = msgInbound.OrderID;
            long messageinboundId = msgInbound.MessageinboundID;
            bool mRes = false;

            try
            {

                DTOOrder mOrder = new DTOOrder();
                DTOOrderLineList mOrderLineList = new DTOOrderLineList();

                mOrder = _orderService.OrderListByID(orderID);

                // For Provider, Check 

                mOrderLineList = _orderService.OrderLineListByOrderID(orderID);

                if (msgInbound.MessageInboundType.ToLower() == "order")
                {

                    DTOOrderSignature mSig = _orderService.OrderSignatureListByOrderID(orderID);

                    if (mSig == null || (mSig != null && string.IsNullOrEmpty(mSig.Path)))
                    {
                        LogMe("ProcessOrderNotification - Null signature path ", EventLogEntryType.Error);

                        UpdateMessageInboundFailed(msgInbound);
                        return false;
                    }

                    DTOSYSConfig SignaturePath = _configService.SYSConfigListByKey("SignatureImagePath");

                    string sigPath = SignaturePath.ConfigValue + "//" + mSig.Path.ToString().Replace("/", "//").ToString();

                    if (sigPath == null) {

                        UpdateMessageInboundFailed(msgInbound);
                        LogMe("Null signature path. OrderID = " + mOrder.OrderID + ", CustomerID =  " + mOrder.CustomerID + ", ProviderID = " + mOrder.ProviderID, EventLogEntryType.Error);
                        return false;

                    }


                    DTOCustomer mCustomer = _customerService.CustomerListByID(mOrder.ProviderID, mOrder.CustomerID);

                    if (mCustomer == null)
                    {
                        UpdateMessageInboundFailed(msgInbound);
                        LogMe("Customer Null OrderID = " + mOrder.OrderID + ", CustomerID =  " + mOrder.CustomerID + ", ProviderID = " + mOrder.ProviderID, EventLogEntryType.Error);
                        return false;
                    }
                    DTOContact customerContact = _customerService.ContactListByID(mCustomer.ContactID);

                    DTOSalesOrg salesOrg = _customerService.SalesOrgListByID(mOrder.SalesOrgID);
                    DTOProviderWarehouse pwarehouse = _orderLincService.ProviderService.ProviderWarehouseListByID(mOrder.ProviderWarehouseID);
                    DTOContact salesOrgContact = _customerService.ContactListBySalesOrgID(mOrder.SalesOrgID);
                    DTOContact salesRepContact = _accountService.ContactListByAccountID(mOrder.SalesRepAccountID);

                    // Get contactID for Provider
                    DTOContact distributorContact = _providerService.ContactListByProviderWareHouseID(mOrder.ProviderWarehouseID);

                    DTOAddress customerAddress = _customerAddress.AddressListByID(mCustomer.AddressID);
                    ReportHeader reportHeader = new ReportHeader()
                    {
                        SalesOrgName = salesOrg != null ? salesOrg.SalesOrgName : string.Empty,
                        SalesOrgPhone = salesOrgContact != null ? salesOrgContact.Phone : string.Empty,
                        ProviderWareHouse = pwarehouse != null ? pwarehouse.ProviderWarehouseName : string.Empty,
                        SalesRepName = salesRepContact != null ? salesRepContact.FullName : string.Empty,
                        StoreManagerName = customerContact.FullName,

                    };

                    if (salesOrgContact != null && string.IsNullOrEmpty(salesOrgContact.Phone))
                        reportHeader.SalesOrgPhone = salesOrgContact.Phone;

                    DTOProvider _provider = _providerService.ProviderListByID(mOrder.ProviderID);
                    _reportService.IsPepsiCoDistributor = _provider.IsPepsiDistributor;
                    string pdfpath = _reportService.RenderCheckoutForm(reportHeader, mCustomer, mOrder, mOrderLineList, customerAddress, sigPath); // Generate PDF file and return the path


                    if (string.IsNullOrEmpty(pdfpath))
                    {
                        EventLog.WriteEntry("OrderLinc", "No file signature was found." + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        LogMe("ProcessOrderNotification - No file signature was found " + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        UpdateMessageInboundFailed(msgInbound);
                        return false;
                    }
                    if (customerContact == null || (customerContact != null && string.IsNullOrEmpty(customerContact.Email)))
                    {
                        UpdateMessageInboundFailed(msgInbound);
                        return false;
                    }

                    string body = CreateEmail(customerContact, mCustomer.CustomerName);
                    DTOProvider mDTOProvider = _providerService.ProviderListByID(mCustomer.ProviderID);


                    string ProviderName = "";

                    if (mDTOProvider != null)
                    {

                        ProviderName = mDTOProvider.ProviderName;
                    }
                    else
                    {
                        ProviderName = "";
                    }

                    string subject = "Order Confirmation - Order No. " + mOrder.OrderNumber + " to " + ProviderName;

                    if (mDTOProvider.IsPepsiDistributor != null)
                    {
                        if (mDTOProvider.IsPepsiDistributor == true)
                        {
                            subject = "Order Confirmation - Order No. " + mOrder.OrderNumber + " to " + pwarehouse.ProviderWarehouseName;
                        }
                    }
                    

                    // 1. customer
                    if (customerContact != null && !string.IsNullOrEmpty(customerContact.Email))
                    {
                        
                        mRes = SendMail(mOrder, customerContact.Email, body, subject, pdfpath);

                        if (mRes == false) // if the customer email is error then exit
                        {
                            LogMe("ProcessOrderNotification - Customer Email failed to send to - " + customerContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);

                            UpdateMessageInboundFailed(msgInbound);
                        }
                    }
                    // send mail cc

                    if (!string.IsNullOrEmpty(MailCC))
                    {
                       mRes = SendMail(mOrder, MailCC, body, subject, pdfpath);
                       if (mRes == false) {

                           LogMe("ProcessOrderNotification - MailCC Email failed to send to - " + customerContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);
                       }
                    }

                    // 2. sales rep

                    if (salesRepContact != null && !string.IsNullOrEmpty(salesRepContact.Email))
                    {
                      mRes =  SendMail(mOrder, salesRepContact.Email, body, subject, pdfpath);

                      if (mRes == false) {

                          LogMe("ProcessOrderNotification - SalesRep Email failed to send to - " + salesRepContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);
                          
                      }
                    }

                    // 3. sales org
                    long salesContactID = 0;
                    if (salesOrgContact != null && !string.IsNullOrEmpty(salesOrgContact.Email))
                    {
                        salesContactID = salesOrgContact.ContactID;
                      mRes =  SendMail(mOrder, salesOrgContact.Email, body, subject, pdfpath);

                      if (mRes == false) {
                          LogMe("ProcessOrderNotification - SalesOrg Email failed to send to - " + salesRepContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);

                          //if (mDTOProvider.IsPepsiDistributor == true && mDTOProvider.IsPepsiDistributor != null)
                          //{
                          //    this.SendMailNotification(salesOrgContact.Email, MailCC, "FAILED SALES ORG EMAIL: " + salesRepContact.Email, "Sales Organization Email failed to send to - " + salesOrgContact.Email + " | OrderNumber :  " + mOrder.OrderNumber + " | Sales Organization : " + salesOrg.SalesOrgName, pdfpath);
                          //}
                      }
                    }

                    bool _isRegularOrder = (mOrder.IsRegularOrder != null && mOrder.IsRegularOrder == true);

                    string distBody = CreateEmailBodyForDistributor(pwarehouse.ProviderWarehouseName, mCustomer.CustomerName, _isRegularOrder);
                    // 4. PROVISION FOR PROVIDER (DISTRIBUTOR)?
                    if (mDTOProvider.IsPepsiDistributor != null && mDTOProvider.IsPepsiDistributor == true)
                    {

                        if ((mOrder.IsRegularOrder != null) && (mOrder.IsRegularOrder == false))
                        {
                            subject = "Pre-sell " + subject;                           
                        }


                        mRes = SendMail(mOrder, distributorContact.Email, distBody, subject, pdfpath);

                        //EventLog.WriteEntry("OrderLinc", "Distributor Email : " + distributorContact.Email, EventLogEntryType.Information);

                        if (mRes == false)
                        {
                            LogMe("ProcessOrderNotification - Distributor Email failed to send to - " + distributorContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);

                            //if (mDTOProvider.IsPepsiDistributor == true && mDTOProvider.IsPepsiDistributor != null)
                            //{
                            //    this.SendMailNotification(salesOrgContact.Email, MailCC, "FAILED DISTRIBUTOR EMAIL: " + distributorContact.Email, "Distributor Email failed to send to - " + distributorContact.Email + " | OrderNumber :  " + mOrder.OrderNumber + " | Sales Organization : " + salesOrg.SalesOrgName, pdfpath);
                            //}

                            UpdateMessageInboundFailed(msgInbound);
                        }

                    }

                    // sales org office admin
                    //DTOContactList officeAdmins = _customerService.ContactListByAccountTypeSalesOrgID(mOrder.SalesOrgID, AccountType.OfficeAdmin);

                    //foreach (var officeAdmin in officeAdmins)
                    //{

                    //    if (officeAdmin != null && !string.IsNullOrEmpty(officeAdmin.Email))
                    //    {
                    //        if (officeAdmin.ContactID == salesContactID) continue;

                    //        SendMail(mOrder, officeAdmin.Email, body, subject, pdfpath);
                    //    }

                    //}

                    if (File.Exists(pdfpath))
                    {
                        try
                        {
                            File.Delete(pdfpath);
                        }
                        catch { }
                    }

                }
                else if (msgInbound.MessageInboundType.ToLower() == "storeid") // It was written for Club Sales but no longer used. 7/15/14
                {
                    try
                    {
                        DTOContact salesOrgContact = _customerService.ContactListBySalesOrgID(mOrder.SalesOrgID);
                        DTOCustomer mCustomer = _customerService.CustomerListByID(mOrder.ProviderID, mOrder.CustomerID);
                        DTOProvider mDTOProvider = _providerService.ProviderListByID(mCustomer.ProviderID);


                        string ProviderName = "";

                        if (mDTOProvider != null)
                        {
                            ProviderName = mDTOProvider.ProviderName;
                        }
                        else
                        {
                            ProviderName = "";
                        }

                        string subject = "Order Confirmation - Order No. " + orderID + " to " + ProviderName;
                        subject = " OrderLinc - The Store Id was changed for " + mCustomer.BusinessNumber + " - " + mCustomer.CustomerName;


                        StringBuilder mBody = new StringBuilder();

                        mBody.Append("Dear  " + salesOrgContact.FirstName + ",").AppendLine();
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine(mCustomer.BusinessNumber + " - " + mCustomer.CustomerName);
                        mBody.AppendLine("Customer Code " + mCustomer.CustomerCode + " [" + ProviderName + "]");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("Customer code has changed: Customer Code (" + mCustomer.CustomerCode + ")");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("OrderLinc");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine(EmailFooter);

                        if (IsValidEmail(salesOrgContact.Email))
                        {
                            _emailService.SendMail(salesOrgContact.Email, "", "", mBody.ToString(), false);

                            mRes = true;

                        }
                        else
                        {

                            LogMe("ProcessOrderNotification - Invalid email - " + salesOrgContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        }
                    }
                    catch
                    {

                        LogMe("ProcessOrderNotification - StoreID Type " + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        UpdateMessageInboundFailed(msgInbound);
                        return false;
                    }
                }
                else   // This is for contactdetails when changed. It was written for Club Sales but no longer used. 7/15/14
                {
                    try
                    {
                        DTOContact salesOrgContact = _customerService.ContactListBySalesOrgID(mOrder.SalesOrgID);
                        DTOCustomer mCustomer = _customerService.CustomerListByID(mOrder.ProviderID, mOrder.CustomerID);
                        DTOProvider mDTOProvider = _providerService.ProviderListByID(mCustomer.ProviderID);
                        DTOContact customerContact = _customerService.ContactListByID(mCustomer.ContactID);

                        string ProviderName = "";

                        if (mDTOProvider != null)
                        {
                            ProviderName = mDTOProvider.ProviderName;
                        }
                        else
                        {
                            ProviderName = "";
                        }

                        string subject = "Order Confirmation - Order No. " + orderID + " to " + ProviderName;

                        subject = " OrderLinc - Change in Customer details for " + mCustomer.BusinessNumber + " - " + mCustomer.CustomerName;
                        StringBuilder mBody = new StringBuilder();

                        mBody.Append("Dear  " + salesOrgContact.FirstName + ",").AppendLine();
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine(mCustomer.BusinessNumber + " - " + mCustomer.CustomerName);
                        mBody.AppendLine("Customer Code (" + mCustomer.CustomerCode + ") [" + ProviderName + "]");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("Contact Details have changed:");
                        mBody.AppendLine("");
                        mBody.AppendLine(customerContact.FirstName + " " + customerContact.LastName);
                        mBody.AppendLine(customerContact.Email);
                        mBody.AppendLine(customerContact.Phone);
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine("OrderLinc");
                        mBody.AppendLine("");
                        mBody.AppendLine("");
                        mBody.AppendLine(EmailFooter);

                        if (IsValidEmail(salesOrgContact.Email))
                        {
                            _emailService.SendMail(salesOrgContact.Email, "", "", mBody.ToString(), false);
                            mRes = true;
                        }
                        else
                        {

                            LogMe("ProcessOrderNotification - Invalid email - " + salesOrgContact.Email + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        }
                    }
                    catch
                    {

                        LogMe("ProcessOrderNotification - StoreID Type " + " | OrderID :  " + orderID, EventLogEntryType.Error);
                        UpdateMessageInboundFailed(msgInbound);
                        return false;
                    }

                }


                return mRes;

            }
            catch (Exception ex)
            {

                LogMe("ProcessOrderNotification Email notification error - " + ex.Message + " " + ex.InnerException.Message, EventLogEntryType.Error);
                UpdateMessageInboundFailed(msgInbound);
                return false;
            }
        }

        private bool SendMail(DTOOrder order, string email, string body, string subject, string attachPDFPath)
        {
            try
            {
                bool ret = false;

                if (string.IsNullOrEmpty(email))
                    return ret;

                if (IsValidEmail(email))
                {

                    string[] _emailAddresses;

                    _emailAddresses = email.Split(',');

                    for (int indx = 0; indx < _emailAddresses.Length; indx++)
                    {
                        ret = _emailService.SendMailWithAttachment(_emailAddresses[indx], "", subject, body, false, attachPDFPath);

                        if (ret == false)
                        {
                            EventLog.WriteEntry("OrderLinc", "Invalid Email Address - " + email + " CustomerID = " + order.CustomerID, EventLogEntryType.Error);
                        }

                    }

                }
                return ret;
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry("OrderLinc", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }

        private bool SendMailNotification(string email, string mailCC, string subject, string body, string pdfPath)
        {
            try
            {
                bool ret = false;

                if (string.IsNullOrEmpty(email))
                    return ret;

                if (IsValidEmail(email))
                {
                    ret = _emailService.SendMailWithAttachment(email, mailCC, subject, body, false, pdfPath);

                    if (ret == false)
                    {
                        EventLog.WriteEntry("OrderLinc", "SendMailNotification: Invalid Email Address - " + email, EventLogEntryType.Error);
                    }

                }
                return ret;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("OrderLinc", ex.Message, EventLogEntryType.Error);
                return false;
            }
        }


        private bool IsValidEmail(string emailAddress)
        {

            // Return true if emailAddress is in valid e-mail format.
            try
            {
                // return Regex.IsMatch(emailAddress, @"^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");

                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("OrderLinc", "Invalid Email Address : " + emailAddress, EventLogEntryType.Warning);   
                return false;
            }
        }

        public void UpdateMessageInboundFailed(DTOMessageInbound msgInbound)
        {
            msgInbound.SentFlag = false;
            msgInbound.Error = true;
            msgInbound.DateSent = null;
            msgInbound.MessageInboundType = "Order";
            _orderService.MessageInboundSaveRecord(msgInbound);
        }

        public void UpdateMessageInboundSuccess(DTOMessageInbound msgInbound)
        {
            msgInbound.SentFlag = true;
            msgInbound.Error = false;
            msgInbound.DateSent = DateTime.Now;
            msgInbound.MessageInboundType = "Order";

            _orderService.MessageInboundSaveRecord(msgInbound);
        }

        public DTOSYSConfig ServiceInterval { get; private set; }
        public DTOSYSConfig PDFImageFooter { get; private set; }
    }
}
