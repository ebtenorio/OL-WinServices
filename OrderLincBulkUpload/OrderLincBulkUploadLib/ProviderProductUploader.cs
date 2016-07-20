using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrderLinc.IDataContracts;
using OrderLinc.Utilities;
using System.Transactions;
using OrderLinc.DTOs;

namespace OrderLinc.BulkUploadLib
{
    public class ProviderProductUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        ICatalogService _productService;

        public ProviderProductUploader(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
            _productService = orderLincService.CatalogService;
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
                Name = "GTIN",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "ProviderProductCode",
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
                        HasError = true;
                        AddError(row.ValidateMessage, row.LineData, row.LineNo);
                        continue;

                    }
                    string providerCode = row.GetValue<string>("ExternalProviderCode");
                    string gTIN = row.GetValue<string>("GTIN");
                    string productCode = row.GetValue<string>("ProviderProductCode");
                    DateTime? startDate = row.GetValue<DateTime?>("Start Date");
                    DateTime? endDate = row.GetValue<DateTime?>("End Date");

                    DTOProvider provider = GetProvider(providerCode);

                    if (provider == null)
                    {
                        AddError("Provider code '{0}' does not exist.", row.LineData, row.LineNo, providerCode);
                        continue;
                    }

                    DTOProduct product = GetProductByGTIN(gTIN);
                    if (product == null)
                    {
                        AddError("Product GTIN Code '{0}' does not exist.", row.LineData, row.LineNo, gTIN);
                        continue;
                    }

                    DTOProviderProduct providerProduct = _providerServivce.ProviderProductListByProductID(provider.ProviderID, product.ProductID);

                    if (providerProduct == null)
                    {
                        providerProduct = new DTOProviderProduct();
                        providerProduct.StartDate = DateTime.Today;
                        providerProduct.EndDate = new DateTime(9999, 9, 9);
                    }


                    providerProduct.ProviderID = provider.ProviderID;
                    providerProduct.ProductID = product.ProductID;
                    providerProduct.ProviderProductCode = productCode;
                    providerProduct.StartDate = startDate.HasValue ? startDate.Value : providerProduct.StartDate;
                    providerProduct.EndDate = endDate.HasValue ? endDate.Value : providerProduct.EndDate;


                    _providerServivce.ProviderProductSaveRecord(providerProduct);
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
