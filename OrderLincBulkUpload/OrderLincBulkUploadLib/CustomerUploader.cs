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
    public class CustomerUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        private DTOSYSStateList _states;
        private ICustomerService _customerService;


        public CustomerUploader(OrderLincServices orderLincService, string csvContent)
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
                Name = "BusinessNumber",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "CustomerName",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "StateCode",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Office - AddressLine1",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Office - AddressLine2",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Office - CitySuburb",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Office - StateCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Office - PostalZipCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "FirstName",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "LastName",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Email",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Phone",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Mobile",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Fax",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Bill To - AddressLine1",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Bill To - AddressLine2",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Bill To - CitySuburb",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Bill To - StateCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Bill To - PostalZipCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Ship To - AddressLine1",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Ship To - AddressLine2",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Ship To - CitySuburb",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Ship To - StateCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Ship To - PostalZipCode",
                DataType = typeof(string),
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
                        AddError(row.ValidateMessage , row.LineData, row.LineNo);
                        continue;
                    }
                    string businessNumber = row.GetValue<string>("BusinessNumber");
                    string customerName = row.GetValue<string>("CustomerName");
                    string stateCode = row.GetValue<string>("StateCode");

                    long stateID = GetStateID(stateCode);

                    if (stateID == 0)
                    {
                        AddError("State code '{0}' does not exist." , row.LineData, row.LineNo, stateCode);
                        continue;
                    }

                    DTOCustomer customer = _customerService.CustomerListByBusinessNumber(SalesOrgID, businessNumber,0);
                    DTOAddress billAddress = null;
                    DTOAddress officeAddress = null;
                    DTOAddress shipAddress = null;
                    DTOContact contact = null;

                    if (customer != null)
                    {
                        if (customer.BillToAddressID > 0)
                            billAddress = _addressService.AddressListByID(customer.BillToAddressID);
                        if (customer.ShipToAddressID > 0)
                            shipAddress = _addressService.AddressListByID(customer.ShipToAddressID);
                        if (customer.AddressID > 0)
                            officeAddress = _addressService.AddressListByID(customer.AddressID);
                        if (customer.ContactID > 0 )
                            contact = _customerService.ContactListByID (customer.ContactID);
                    }

                    if (customer == null)
                    {
                        customer = new DTOCustomer()
                        {
                            CreatedByUserID = UserID,
                            DateCreated = DateTime.Now
                        };

                    }

                    if (billAddress == null)
                    {
                        billAddress = new DTOAddress()
                        {
                            CreatedByUserID = UserID,

                        };
                    }

                    if (officeAddress == null)
                    {
                        officeAddress = new DTOAddress()
                        {
                            CreatedByUserID = UserID,
                        };
                    }

                    if (shipAddress == null)
                    {
                        shipAddress = new DTOAddress()
                        {
                            CreatedByUserID = UserID,

                        };
                    }

                    if (contact == null)
                    {
                        contact = new DTOContact()
                        {
                            CreatedByUserID = UserID,

                        };
                    }
                    customer.CustomerCode = businessNumber;
                    customer.BusinessNumber = businessNumber;
                    customer.CustomerName = customerName;
                    customer.ProviderID = 0;
                    customer.DateUpdated = DateTime.Now;
                    customer.UpdatedByUserID = UserID;
                    customer.SalesOrgID = SalesOrgID;
                    customer.SYSStateID = stateID;
                    
                    officeAddress.AddressLine1 = row.GetValue<string>("Office - AddressLine1");
                    officeAddress.AddressLine2 = row.GetValue<string>("Office - AddressLine2");
                    officeAddress.CitySuburb = row.GetValue<string>("Office - CitySuburb");
                    officeAddress.PostalZipCode = row.GetValue<string>("Office - PostalZipCode");
                    officeAddress.SYSStateID = GetStateID(row.GetValue<string>("Office - StateCode"));
                    officeAddress.AddressTypeID = 2;

                    billAddress.AddressTypeID = 3;
                    billAddress.AddressLine1 = row.GetValue<string>("Bill To - AddressLine1");
                    billAddress.AddressLine2 = row.GetValue<string>("Bill To - AddressLine2");
                    billAddress.CitySuburb = row.GetValue<string>("Bill To - CitySuburb");
                    billAddress.PostalZipCode = row.GetValue<string>("Bill To - PostalZipCode");
                    billAddress.SYSStateID = GetStateID(row.GetValue<string>("Bill To - StateCode"));

                    shipAddress.AddressTypeID = 5;
                    shipAddress.AddressLine1 = row.GetValue<string>("Ship To - AddressLine1");
                    shipAddress.AddressLine2 = row.GetValue<string>("Ship To - AddressLine2");
                    shipAddress.CitySuburb = row.GetValue<string>("Ship To - CitySuburb");
                    shipAddress.PostalZipCode = row.GetValue<string>("Ship To - PostalZipCode");
                    shipAddress.SYSStateID = GetStateID(row.GetValue<string>("Ship To - StateCode"));


                    contact.Email = GetNonEmpty(row.GetValue<string>("Email"), contact.Email);
                    contact.Mobile = GetNonEmpty(row.GetValue<string>("Mobile"), contact.Mobile);
                    contact.Fax = GetNonEmpty(row.GetValue<string>("Fax"), contact.Fax);
                    contact.Phone = GetNonEmpty(row.GetValue<string>("Phone"), contact.Phone);
                    contact.LastName = GetNonEmpty(row.GetValue<string>("LastName"), contact.LastName);
                    contact.FirstName = GetNonEmpty(row.GetValue<string>("FirstName"), contact.FirstName);
                    
                    using (TransactionScope scope = new TransactionScope( ))
                    {
                        if (contact.ContactID > 0 || !contact.IsEmpty)
                            _customerService.ContactSaveRecord(contact);

                        if (officeAddress.AddressID > 0 || !officeAddress.IsEmpty)
                            _addressService.AddressSaveRecord(officeAddress);

                        if (billAddress.AddressID > 0 || !billAddress.IsEmpty)
                            _addressService.AddressSaveRecord(billAddress);

                        if (shipAddress.AddressID > 0 || !shipAddress.IsEmpty)
                            _addressService.AddressSaveRecord(shipAddress);

                        customer.AddressID = officeAddress.AddressID;
                        customer.ShipToAddressID = shipAddress.AddressID;
                        customer.BillToAddressID = billAddress.AddressID;
                        customer.ContactID = contact.ContactID;

                        _customerService.CustomerSaveRecord(customer);

                        scope.Complete();
                    }

                }
                catch (Exception ex)
                {
                    AddError(ex.Message, row.LineData, row.LineNo);
                    HasError = true;
                }
                LineCountProcessed += 1;
            }

        }

    }
}
