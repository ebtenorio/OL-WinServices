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
    public class ProductUploader : DataUpload
    {
        IProviderService _providerServivce;
        ICatalogService _catalogService;

        IAddressService _addressService;
        //private DTOProviderList _providers;
        private DTOProductUOMList _uolList;

        public ProductUploader(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
            _catalogService = orderLincService.CatalogService;

            AddFields();
        }

        private void AddFields()
        {
            this.Fields.Add(new CsvField()
            {
                Name = "ProductGroupText",
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
                Name = "ProductDescription",
                DataType = typeof(string),
                IsRequired = true
            });

            this.Fields.Add(new CsvField()
            {
                Name = "PackingUnits",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "UOM",
                DataType = typeof(string),
                IsRequired = true
            });
        }

        private long GetUOMID(string uom)
        {
            if (_uolList == null || (_uolList != null && _uolList.Count > 0))
                _uolList = _catalogService.ProductUOMList();

            DTOProductUOM found = _uolList.Where(p => p.ProductUOM.Equals(uom, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (found == null)
            {
                found = new DTOProductUOM()
                {
                    ProductUOM = uom
                };
                _catalogService.ProductUOMSaveRecord(found);
            }


            return found.ProductUOMID;

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

                    string groupText = row.GetValue<string>("ProductGroupText");

                    string gTINCode = row.GetValue<string>("GTIN");
                    //string providerCode = row.GetValue<string>("ProviderCode");
                    string productName = row.GetValue<string>("ProductDescription");
                    int groupSortOrder = 0;//row.GetValue<int>("ProductGroupSortOrder"); 
                    int productSortOrder = 0;// row.GetValue<int>("ProductSortOrder");
                    string uom = row.GetValue<string>("UOM");
                    string packingSKU = row.GetValue<string>("PackingUnits");


                    DTOProduct product = _catalogService.ProductListByGTINCode(SalesOrgID, gTINCode);

                    DTOProductGroup pGroup = _catalogService.ProductGroupListByProductGroupText(SalesOrgID, groupText);

                    if (pGroup == null)
                    {
                        pGroup = new DTOProductGroup()
                        {
                            InActive = false,
                            ProductGroupText = groupText,
                            SalesOrgID = SalesOrgID,
                            SortPosition = groupSortOrder,
                        };

                        _catalogService.ProductGroupSaveRecord(pGroup);
                    }

                    bool createNewGroupLine = false;
                    bool productUpdateOrCreate = false;
                    long uomID = GetUOMID(uom);

                    if (product == null)
                    {
                        createNewGroupLine = true;
                        productUpdateOrCreate = true;
                        product = new DTOProduct()
                        {
                            GTINCode = gTINCode,
                            Inactive = false,
                            PrimarySKU = 0,
                            ProductBrandID = 0,
                        };
                    }
                    else
                    {
                        // retrieve the product group relationship
                        // compare product group if the same otherwise remove the exist assigment.
                        DTOProductGroupLine pGroupLine = _catalogService.ProductGroupLineListByProductID(product.ProductID);
                        if (pGroupLine == null)
                        {
                            createNewGroupLine = true;
                        }
                        else if (pGroupLine.ProductGroupID != product.ProductID)
                        {
                            _catalogService.ProductGroupLineDeleteRecord(pGroupLine.ProductGroupLineID);
                            createNewGroupLine = true;
                        }

                        if (product.ProductDescription != productName || product.GTINCode != gTINCode || product.UOMID != uomID)
                        {
                            productUpdateOrCreate = true;
                        }
                    }

                    product.GTINCode = gTINCode;
                    product.ProductDescription = productName;
                    product.ProviderID = 0;
                    product.SalesOrgID = SalesOrgID;
                    product.UOMID = uomID;

                    long sku = 0;

                    long.TryParse(packingSKU , out sku);

                    product.PrimarySKU = sku;

                    using (TransactionScope scope = new TransactionScope())
                    {
                        if (productUpdateOrCreate)
                        {
                            _catalogService.ProductSaveRecord(product);
                        }
                        if (createNewGroupLine)
                        {
                            DTOProductGroupLine pGroupLine = new DTOProductGroupLine()
                            {
                                ProductGroupID = pGroup.ProductGroupID,
                                ProductID = product.ProductID,
                                SortPosition = productSortOrder,
                                DefaultQty = 0
                            };
                            _catalogService.ProductGroupLineSaveRecord(pGroupLine);
                        }
                        scope.Complete();
                    }
                    LineCountProcessed += 1;
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
