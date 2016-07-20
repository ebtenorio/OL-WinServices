using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OrderLinc.Utilities;
using OrderLinc.DTOs;
using OrderLinc.IDataContracts;

namespace OrderLinc.BulkUploadLib
{
    public abstract class DataUpload : IDisposable
    {

        string _csvContent;

        private IProviderService _providerService;
        private IAddressService _addressService;
        private ICustomerService _customerService;
        private IAccountService _accountService;

        private DTOProductList _productCache;
        private DTOAccountList _accountCache;
        private DTOCustomerList _customerCache;
        private DTOSYSStateList _stateCache;
        private DTOProviderList _providerCache;
        private ICatalogService _productService;


        #region Constructor

        public DataUpload(OrderLincServices service, string csvPath)
        {
            try
            {
                OrderLincServices = service;
                CsvPath = csvPath;
                _csvContent = File.ReadAllText(csvPath);

                Init();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Init()
        {
            ErrorBuilder = new ErrorStringBuilder();
            _providerService = OrderLincServices.ProviderService;
            _addressService = OrderLincServices.AddressService;
            _customerService = OrderLincServices.CustomerService;
            _accountService = OrderLincServices.AccountService;
            _productService = OrderLincServices.CatalogService;

            _accountCache = new DTOAccountList();
            _customerCache = new DTOCustomerList();
            _providerCache = new DTOProviderList();
            _productCache = new DTOProductList();

            Fields = new List<CsvField>();
        }

        #region Processing methods

        private void ReadStream(Stream dataStream)
        {
            if (dataStream == null) throw new ArgumentNullException("csvData");
            if (dataStream.Length == 0) throw new ArgumentNullException("empty");

            byte[] byteData = new byte[dataStream.Length];

            dataStream.Read(byteData, 0, byteData.Length);

            ReadBytes(byteData);
        }

        private void ReadBytes(byte[] dataBytes)
        {
            if (dataBytes == null) throw new ArgumentNullException("dataBytes");
            if (dataBytes.Length == 0) throw new ArgumentException("empty");

            _csvContent = Encoding.UTF8.GetString(dataBytes);
        }

        #endregion


        #endregion

        #region Properties

        public bool IsEmpty
        {
            get
            {
                return _csvContent.Length == 0;
            }
        }
        protected OrderLincServices OrderLincServices { get; private set; }

        public string FileName { get { return Path.GetFileName(CsvPath); } }

        public string CsvPath { get; private set; }

        //public bool Success { get; private set; }

        public string Error { get { return ErrorBuilder.ToString(); } }

        public long SalesOrgID { get; set; }

        public long UserID { get; set; }

        public List<CsvField> Fields { get; set; }

        public bool HasError { get; protected set; }
        #endregion

        public string GetNonEmpty(string valueExpression, string value)
        {
            if (string.IsNullOrEmpty(valueExpression)) return value;

            return valueExpression;
        }

        public void Upload()
        {
            HasError = false;
            if (IsEmpty)
            {
                HasError = true;
                // Success = false;
                AddError("File is empty.");
                return;
            }
            if (_csvContent.Length < 10) // just put a threshold for invalid csv
            {
                HasError = true;
                //Success = false;
                AddError("The file is empty or invalid format.");
                return;
            }

            CsvReader csvReader = new CsvReader(_csvContent);
            if (Fields != null && Fields.Count > 0)
                csvReader.Fields = Fields;

            csvReader.RowReadEvent += new EventHandler<CsvReaderRowArgs>(csvReader_RowReadEvent);

            csvReader.Read();

            if (csvReader.IsValid == false)
            {
                HasError = true;
                AddError(csvReader.ErrorText);
                return;
            }
            else if (csvReader.Rows.Count == 0 || csvReader.Fields.Count == 0)
            {
                //Success = false;
                HasError = true;
                AddError("Empty file.");
                return;
            }

            LineCount = csvReader.Rows.Count;
            UploadData(csvReader);

        }

        public int LineCountProcessed { get; protected set; }

        public int LineCount { get; private set; }

        void csvReader_RowReadEvent(object sender, CsvReaderRowArgs e)
        {

        }

        protected abstract void UploadData(CsvReader reader);

        private ErrorStringBuilder ErrorBuilder { get; set; }

        public bool IsValid
        {
            get
            {
                return ErrorBuilder.IsEmpty;
            }
        }
        public void AddError(string message)
        {
            //ErrorBuilder.AddError("{0} [{1}] - {2} ", FileName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), message);
            ErrorBuilder.AddError("{0} : {1} ", FileName, message);
            HasError = true;
        }

        public void AddError(string message, int lineNo)
        {
            //ErrorBuilder.AddError("{0} [{1}] [{2}] - {3} ", FileName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), lineNo, message);
            ErrorBuilder.AddError("{0} [{1}] : {2} ", FileName, lineNo, message);
            HasError = true;
        }

        public void AddError(string message, string lineData, int lineNo)
        {
            // ErrorBuilder.AddError("{0} [{1}] [{2}] - {3} ", FileName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), lineNo, message);
            ErrorBuilder.AddError("{0} [{1}] : {2} : {3}", FileName, lineNo, message, lineData);
            HasError = true;
        }

        public void AddError(string message, string lineData, int lineNo, params object[] args)
        {
            string msg = string.Format(message, args);
            HasError = true;

            //ErrorBuilder.AddError("{0} [{1}] [{2}] - {3} ", FileName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), lineNo, msg);
            ErrorBuilder.AddError("{0} [{1}] : {2} : {3} ", FileName, lineNo, msg, lineData);
        }


        public void AddError(string message, int lineNo, params object[] args)
        {
            string msg = string.Format(message, args);
            HasError = true;
            //ErrorBuilder.AddError("{0} [{1}] [{2}] - {3} ", FileName, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), lineNo, msg);
            ErrorBuilder.AddError("{0} [{1}] : {2} ", FileName, lineNo, msg);
        }

        #region Lookup cache methods

        public int GetStateID(string stateCode)
        {
            if (_stateCache == null)
                _stateCache = _addressService.SYSStateList();

            var state = _stateCache.Where(p => p.StateCode.Equals(stateCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (state == null) return 0;

            return state.SYSStateID;
        }

        public DTOAccount GetAccount(string userName)
        {
            var account = _accountCache.Where(p => p.RefID == SalesOrgID && p.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (account == null)
            {
                account = _accountService.AccountListByUsername(SalesOrgID, userName);
                if (account != null)
                    _accountCache.Add(account);
            }

            return account;
        }

        public DTOAccount GetAccount(string userName, string password)
        {
            var account = _accountCache.Where(p => p.RefID == SalesOrgID &&
                p.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
                p.Password.Equals(password, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (account == null)
            {
                account = _accountService.AccountListByUsername(SalesOrgID, userName, password);
                if (account != null)
                    _accountCache.Add(account);
            }

            return account;
        }


        public Int16 CheckNewAccountDetails(long salesorgid, string userName, string password, long userid)
        {
            // return _accountService.IsAccountExistingAcrossAllOtherSalesOrgs(salesorgid, userName, password);
            return _accountService.CheckNewAccountDetails(userName, password, salesorgid.ToString(), userid.ToString());

        }

        /// <summary>
        /// Retrieve from cache or from db if not in the cache filtered with sales org.
        /// </summary>
        /// <param name="businessNumber"></param>
        /// <returns></returns>
        public DTOCustomer GetCustomerByBusinessNumber(string businessNumber)
        {
            var customer = _customerCache.Where(p => p.BusinessNumber.Equals(businessNumber, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (customer == null)
            {
                customer = _customerService.CustomerListByBusinessNumber(SalesOrgID, businessNumber,0);
                if (customer != null)
                    _customerCache.Add(customer);
            }

            return customer;
        }

        public DTOProvider GetProvider(string providerCode)
        {
            if (_productCache.Count == 0)
            {
                _providerCache = _providerService.ProviderListBySalesOrgID(SalesOrgID);
            }

            var provider = _providerCache.Where(p => p.BusinessNumber.Equals(providerCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            return provider;
        }

        public DTOProduct GetProductByGTIN(string gTINCode)
        {

            var product = _productCache.Where(p => p.GTINCode.Equals(gTINCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (product == null)
            {
                product = _productService.ProductListByGTINCode(SalesOrgID, gTINCode);
                if (product != null)
                    _productCache.Add(product);
            }

            return product;
        }

        #endregion

        public void Dispose()
        {
            _accountCache = null;
            _customerCache = null;
            _stateCache = null;
        }
    }
}
