using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrderLinc.IDataContracts;
using OrderLinc.DTOs;
using OrderLinc.Utilities;
using System.Transactions;

namespace OrderLinc.BulkUploadLib
{
    public class CustomerSalesRepUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        private DTOSYSStateList _states;
        private ICustomerService _cutomerService;
        private IAccountService _accountService;

        public CustomerSalesRepUploader(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
            _accountService = orderLincService.AccountService;
            _cutomerService = orderLincService.CustomerService;

            AddFields();

        }

        private void AddFields()
        {
            this.Fields.Add(new CsvField()
            {
                Name = "UserName",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "BusinessNumber",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Start Date",
                DataType = typeof(DateTime?),
                Format = "yyyyMMdd"
            });
            this.Fields.Add(new CsvField()
            {
                Name = "End Date",
                DataType = typeof(DateTime?),
                Format = "yyyyMMdd"
            });
        }

        protected override void UploadData(CsvReader reader)
        {

            foreach (var row in reader.Rows)
            {
                try
                {
                    if (!row.IsValid)
                    {
                        AddError(row.ValidateMessage, row.LineData, row.LineNo);
                        continue;
                    }
                    string userName = row.GetValue<string>("UserName");
                    string businessNumber = row.GetValue<string>("BusinessNumber");
                    DateTime? startDate = row.GetValue<DateTime?>("Start Date");
                    DateTime? endDate = row.GetValue<DateTime?>("End Date");

                    DTOAccount account = GetAccount(userName);
                    if (account == null)
                    {
                        AddError("Error Invalid userName. Username '{0}' does not exist.", row.LineData, row.LineNo, userName);
                        continue;
                    }

                    DTOCustomer customer = GetCustomerByBusinessNumber(businessNumber);
                    if (customer == null)
                    {
                        AddError("Error Invalid Business Number. Customer business number '{0}' does not exist.", row.LineData, row.LineNo, businessNumber);
                        continue;
                    }


                    DTOCustomerSalesRepList saleReps = _cutomerService.CustomerSalesRepListSearchSalesRepAndCustomer(account.AccountID, customer.CustomerID);

                    DTOCustomerSalesRep saleRep = null;// GetCustomerSalesRep(account.AccountID, customer.CustomerID);
                    if (saleReps.Count > 0)
                    {
                        saleRep = saleReps[0];
                    }
                    if (saleRep == null)
                    {
                        saleRep = new DTOCustomerSalesRep();
                        saleRep.StartDate = DateTime.Today;
                        saleRep.EndDate = new DateTime(9999, 9, 9);
                        
                    }
                    saleRep.DateCreated = DateTime.Now;
                    saleRep.CustomerID = customer.CustomerID;
                    saleRep.SalesRepAccountID = account.AccountID;
                    saleRep.StartDate = startDate.HasValue ? startDate.Value : saleRep.StartDate;
                    saleRep.EndDate = endDate.HasValue ? endDate.Value : saleRep.EndDate;
                    _cutomerService.CustomerSalesRepSaveRecord(saleRep);

                }
                catch (Exception ex)
                {
                    AddError(ex.Message, row.LineNo);
                    HasError = true;
                }

                LineCountProcessed += 1;
            }
        }

        DTOCustomerSalesRepList _customerSalesReps = null;

        private DTOCustomerSalesRep GetCustomerSalesRep(long accountID, long customerID)
        {
            if (_customerSalesReps == null)
            {
                _customerSalesReps = _cutomerService.CustomerSalesRepListBySalesRepID(accountID, 0, 2000);
            }

            DTOCustomerSalesRep salesRep = _customerSalesReps.Where(p => p.CustomerID == customerID && p.SalesRepAccountID == accountID).FirstOrDefault();

            return salesRep;
        }
    }
}
