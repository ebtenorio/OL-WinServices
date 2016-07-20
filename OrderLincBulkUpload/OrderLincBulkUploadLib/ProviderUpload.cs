using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OrderLinc.IDataContracts;
using OrderLinc.Utilities;
using OrderLinc.DTOs;
using System.Transactions;

namespace OrderLinc.BulkUploadLib
{
    public class ProviderUpload : DataUpload
    {

        IProviderService _providerServivce;
        IAddressService _addressService;

        public ProviderUpload(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
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
                Name = "ProviderName",
                DataType = typeof(string),
                IsRequired = true
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
                        HasError = true;
                        AddError(row.ValidateMessage , row.LineData, row.LineNo);
                        continue;
                    }

                    string providerCode = row.GetValue<string>("ExternalProviderCode");
                    string providerName = row.GetValue<string>("ProviderName");

                    DTOProvider provider = GetProvider(providerCode);

                    if (provider == null)
                    {
                        provider = new DTOProvider();
                        provider.CreatedByUserID = UserID;
                    }

                    provider.AddressID = 0;
                    provider.SalesOrgID = SalesOrgID;
                    provider.BusinessNumber = providerCode;
                   // provider.ProviderCode = providerCode;
                    provider.ProviderName = providerName;

                    _providerServivce.ProviderSaveRecord(provider);

                    LineCountProcessed++;
                }
                catch (Exception ex)
                {
                    AddError(ex.Message , row.LineData, row.LineNo);
                    HasError = true;
                }
            }
        }
    }
}
