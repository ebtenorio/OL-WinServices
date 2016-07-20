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
    public class ProviderWarehouseUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        private DTOProviderList _providers;


        public ProviderWarehouseUploader(OrderLincServices orderLincService, string csvContent)
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
                Name = "ProviderWarehouseCode",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "ProviderWarehouseName",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Start Date",
                DataType = typeof(DateTime),
                Format = "yyyyMMdd"
            });
            this.Fields.Add(new CsvField()
            {
                Name = "End Date",
                DataType = typeof(DateTime),
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
                    string pwarehouseCode = row.GetValue<string>("ProviderWarehouseCode");
                    string pwarehouseName = row.GetValue<string>("ProviderWarehouseName");
                    DateTime? sDate = row.GetValue<DateTime?>("Start Date");
                    DateTime? eDate = row.GetValue<DateTime?>("End Date");


                    var provider = GetProvider(providerCode);

                    if (provider == null)
                    {
                        AddError("Error Invalid Provider Code. Provider code '{0}' does not exist.", row.LineData, row.LineNo, providerCode);
                        continue;
                    }
                    DTOProviderWarehouse pWarehouse = _providerServivce.ProviderWarehouseListByWarehouseCode(provider.ProviderID, pwarehouseCode);

                    if (pWarehouse == null)
                    {
                        pWarehouse = new DTOProviderWarehouse();
                        pWarehouse.AddressID = 0;
                        pWarehouse.CreatedByUserID = UserID;
                        pWarehouse.StartDate = DateTime.Today;
                        pWarehouse.EndDate = new DateTime(9999, 09, 09);
                        pWarehouse.DateCreated = DateTime.Now;

                    }
                    pWarehouse.ProviderID = provider.ProviderID;
                    pWarehouse.ProviderWarehouseCode = pwarehouseCode;
                    pWarehouse.ProviderWarehouseName = pwarehouseName;
                    pWarehouse.UpdatedByUserID = UserID;
                    pWarehouse.DateUpdated = DateTime.Now;
                    pWarehouse.StartDate = sDate.HasValue ? sDate.Value : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//pWarehouse.StartDate;
                    pWarehouse.EndDate = eDate.HasValue ? eDate.Value : pWarehouse.EndDate;


                    _providerServivce.ProviderWarehouseSaveRecord(pWarehouse);
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
