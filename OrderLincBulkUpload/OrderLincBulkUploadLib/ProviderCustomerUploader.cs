using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrderLinc.IDataContracts;
using OrderLinc.DTOs;
using System.Transactions;
using OrderLinc.Utilities;

namespace OrderLinc.BulkUploadLib
{
    public class ProviderCustomerUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        ICustomerService _customerService;


        public ProviderCustomerUploader(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
            _customerService = orderLincService.CustomerService;

            AddFields();


        }

        private void AddFields()
        {
            this.Fields.Add(new CsvField()
            {
                Name = "ExternalProviderCode",
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
                Name = "ProviderCustomerCode",
                DataType = typeof(string),
                IsRequired = false
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Start Date",
                DataType = typeof(DateTime?),
                Format = "yyyyMdd"
            });
            this.Fields.Add(new CsvField()
            {
                Name = "End Date",
                DataType = typeof(DateTime?),
                Format = "yyyyMdd"
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

                    string providerCode = row.GetValue<string>("ExternalProviderCode");
                    string businessNumber = row.GetValue<string>("BusinessNumber");
                    string customerCode = row.GetValue<string>("ProviderCustomerCode");
                    DateTime? startDate = row.GetValue<DateTime?>("Start Date");
                    DateTime? endDate = row.GetValue<DateTime?>("End Date");


                    DTOCustomer customer = GetCustomerByBusinessNumber(businessNumber);

                    if (customer == null)
                    {
                        AddError("Error Invalid BusinessNumber. Business Number '{0}' does not exist.", row.LineData, row.LineNo, businessNumber);
                        continue;
                    }

                    DTOProvider provider = GetProvider(providerCode);
                    if (provider == null)
                    {
                        AddError("Error Invalid Provider. Provider Code '{0}' does not exist.", row.LineData, row.LineNo, providerCode);
                        continue;
                    }

                    DTOProviderCustomer pCust = _providerServivce.ProviderCustomerByProviderCustomer(provider.ProviderID, customer.CustomerID);

                    if (pCust == null)
                    {
                        pCust = new DTOProviderCustomer()
                        {
                            CustomerID = customer.CustomerID,
                            ProviderID = provider.ProviderID,
                            StartDate = DateTime.Today,
                            EndDate = new DateTime(9999,9,9),
                            ProviderCustomerCode = customerCode,
                        };
                    }
                    else
                    {
                        // this should not be happening
                        if (pCust.StartDate.Equals(DateTime.MinValue))
                            pCust.StartDate = DateTime.Today;
                        if (pCust.EndDate.Equals(DateTime.MinValue))
                            pCust.EndDate = new DateTime(9999, 9, 9);
                    }


                    pCust.CustomerID = customer.CustomerID;
                    pCust.ProviderID = provider.ProviderID;
                    pCust.StartDate = startDate.HasValue ? startDate.Value : pCust.StartDate;
                    pCust.EndDate = endDate.HasValue ? endDate.Value : pCust.EndDate;

                    if (!string.IsNullOrEmpty(customerCode))
                        pCust.ProviderCustomerCode = customerCode;



                    _providerServivce.ProviderCustomerSaveRecord(pCust);
                    LineCountProcessed++;
                }
                catch (Exception ex)
                {
                    AddError(ex.Message, row.LineData, row.LineNo);
                    HasError = true;
                }
            }
        }

    }
}
