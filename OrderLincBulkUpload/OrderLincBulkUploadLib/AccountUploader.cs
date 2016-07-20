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
    public class AccountUploader : DataUpload
    {
        IProviderService _providerServivce;
        IAddressService _addressService;
        IAccountService _accountService;
        ICustomerService _customerService;

        DTOAccountList _accountCache;

        int _orgUnitID;
        private DTOAccountTypeList _accountTypeCache;

        public AccountUploader(OrderLincServices orderLincService, string csvContent)
            : base(orderLincService, csvContent)
        {

            _providerServivce = orderLincService.ProviderService;
            _addressService = orderLincService.AddressService;
            _accountService = orderLincService.AccountService;
            _customerService = orderLincService.CustomerService;
            _accountCache = new DTOAccountList();

            AddFields();
        }

        private int GetOrgUnitID()
        {
            if (_orgUnitID == 0)
            {
                DTOOrgUnitList orgUnitList = _customerService.OrgUnitListBySalesOrgID(SalesOrgID);

                if (orgUnitList.Count > 0) _orgUnitID = orgUnitList[0].OrgUnitID;
            }

            return _orgUnitID;
        }

        private int GetAccountTypeID(string accountType)
        {
            if (_accountTypeCache == null)
                _accountTypeCache = _accountService.AccountTypeList();

            var aType = _accountTypeCache.Where(p => p.AccountTypeCode.Equals(accountType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (aType == null)
                return 0;

            return aType.AccountTypeID;
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
                Name = "Password",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "Account Type",
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
            this.Fields.Add(new CsvField()
            {
                Name = "AddressLine1",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "AddressLine2",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "CitySuburb",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "StateCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "PostalZipCode",
                DataType = typeof(string),
            });
            this.Fields.Add(new CsvField()
            {
                Name = "FirstName",
                DataType = typeof(string),
                IsRequired = true
            });
            this.Fields.Add(new CsvField()
            {
                Name = "LastName",
                DataType = typeof(string),
                IsRequired = true
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

        }

        protected override void UploadData(CsvReader reader)
        {
            try
            {
                DateTime sDate = DateTime.Now;
                DateTime eDate = new DateTime(9999, 09, 09);

                foreach (var row in reader.Rows)
                {
                    try
                    {
                        if (row.IsValid == false)
                        {
                            HasError = true;
                            AddError(row.ValidateMessage , row.LineData, row.LineNo);
                            continue;
                        }

                        string userName = row.GetValue<string>("UserName");
                        string password = row.GetValue<string>("Password");

                        //if (this.CheckNewAccountDetails(SalesOrgID, userName, password, account.AccountID) == 3)
                        //{
                        //    HasError = true;
                        //    // Password is not available for this username.
                        //    AddError("Password is not available for this username.", row.LineData, row.LineNo);
                        //    continue;
                        //}

                        string accountType = row.GetValue<string>("Account Type");
                        string stateCode = row.GetValue<string>("StateCode");

                        string firstName = row.GetValue<string>("FirstName");
                        string lastName = row.GetValue<string>("LastName");
                        string email = row.GetValue<string>("Email");
                        DateTime? startDate = row.GetValue<DateTime?>("Start Date");
                        DateTime? endDate = row.GetValue<DateTime?>("End Date");

                        int accountTypeID = GetAccountTypeID(accountType);
                        if (accountTypeID == 0)
                        {
                            AddError("Invalid account type." , row.LineData, row.LineNo);
                            continue;
                        }
                        int stateID = 0;
                        if (string.IsNullOrEmpty(stateCode) == false)
                        {
                            stateID = GetStateID(stateCode);
                        }

                        DTOAccount account = GetAccount(userName);
                        DTOAddress address = null;
                        DTOContact contact = null;

                        if (account == null)
                        {
                            account = new DTOAccount()
                            {
                                OrgUnitID = GetOrgUnitID(),
                                RefID = SalesOrgID,
                                DeviceNo = string.Empty,
                                StartDate = sDate,
                                EndDate = eDate,
                                CreatedByUserID = UserID,
                                AccountTypeID = 5
                            };

                        }
                        else
                        {

                            if (this.CheckNewAccountDetails(SalesOrgID, userName, password, account.AccountID) == 3)
                            {
                                HasError = true;
                                // Password is not available for this username.
                                AddError("Password is not available for this username.", row.LineData, row.LineNo);
                                continue;
                            }

                            address = _addressService.AddressListByID(account.AddressID);

                            contact = _accountService.ContactListByID(account.ContactID);

                        }
                        if (address == null)
                        {
                            address = new DTOAddress();

                        }
                        if (contact == null)
                        {
                            contact = new DTOContact();
                            contact.CreatedByUserID = UserID;

                        }

                        address.AddressLine1 = row.GetValue<string>("AddressLine1");
                        address.AddressLine2 = row.GetValue<string>("AddressLine2");
                        address.CitySuburb = row.GetValue<string>("CitySuburb");
                        address.PostalZipCode = row.GetValue<string>("PostalZipCode");
                        address.SYSStateID = stateID;
                        address.AddressTypeID = 2;

                        contact.FirstName = firstName;
                        contact.LastName = lastName;
                        contact.Email = email;
                        contact.Fax = row.GetValue<string>("Fax");
                        contact.Mobile = row.GetValue<string>("Mobile");
                        contact.Phone = row.GetValue<string>("Phone");

                        account.LastName = lastName;
                        account.FirstName = firstName;
                        account.StartDate = startDate.HasValue ? startDate.Value : account.StartDate;
                        account.EndDate = endDate.HasValue ? endDate.Value : account.EndDate;
                        account.UpdatedByUserID = UserID;
                        account.AccountTypeID = accountTypeID;
                        account.Username = userName;
                        account.Password = password;
                        
                        account.LastLoginDate = new DateTime(9999, 9, 9);
                        account.DateUpdated = DateTime.Now;
                        account.ServerID = 1;

                        using (TransactionScope scope = new TransactionScope())
                        {
                            if (!address.IsEmpty)
                                _addressService.AddressSaveRecord(address);

                            if (!contact.IsEmpty)
                                _accountService.ContactSaveRecord(contact);

                            account.AddressID = address.AddressID;
                            account.ContactID = contact.ContactID;

                            _accountService.AccountSaveRecord(account);

                            scope.Complete();
                        }

                    }
                    catch (Exception ex)
                    {
                        HasError = true;
                        AddError("{0}." , row.LineData, row.LineNo, ex.Message);
                        continue;

                    }

                    LineCountProcessed += 1;
                }
            }
            catch (Exception ex)
            {
                AddError(string.Format("{0}", ex.Message));
                HasError = true;
            }
        }
    }
}
