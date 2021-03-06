﻿using AS_Licence.Data.Interface.UnitOfWork;
using AS_Licence.Entites.Validation.Entity.Customer;
using AS_Licence.Entities.ViewModel.Operations;
using AS_Licence.Helpers.Extension;
using AS_Licence.Service.Interface.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AS_Licence.Service.Host.Customer
{

  public class CustomerService : ICustomerManager
  {
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public async Task<OperationResponse<List<Entities.Model.Customer.Customer>>> GetCustomerList(
      Expression<Func<Entities.Model.Customer.Customer, bool>> filter = null,
      Func<IQueryable<Entities.Model.Customer.Customer>, IOrderedQueryable<Entities.Model.Customer.Customer>> orderBy =
        null, string includeProperties = "")
    {
      OperationResponse<List<Entities.Model.Customer.Customer>> response = new OperationResponse<List<Entities.Model.Customer.Customer>>();
      try
      {
        response.Data = await _unitOfWork.CustomerRepository.Get(filter, orderBy, includeProperties);
        response.Status = true;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }
      return response;
    }



    public async Task<OperationResponse<Entities.Model.Customer.Customer>> SaveCustomer(
      Entities.Model.Customer.Customer customer)
    {
      OperationResponse<Entities.Model.Customer.Customer> response = new OperationResponse<Entities.Model.Customer.Customer>();

      try
      {
        if (customer == null)
        {
          throw new Exception("Customer nesnesi null olamaz");
        }

        var valid = await new CustomerValidator().ValidateAsync(customer);
        if (valid.IsValid == false)
        {
          throw new Exception(valid.GetErrorMessagesOnSingleLine());
        }

        Entities.Model.Customer.Customer customerExists = null;
        if (customer.CustomerId > 0)
        {
          var existsCustomer = await _unitOfWork.CustomerRepository.GetById(customer.CustomerId);
          customerExists = existsCustomer ?? throw new Exception("Sistemde kayıtlı bir müşteri bilgisi bulunamadı.");
        }
        else
        {
          //Check customer by name
          //İf exists good bye..
          var existsCustomerByCustomerEMail = await _unitOfWork.CustomerRepository.GetCustomerByEmail(customer.CustomerEMail);
          if (existsCustomerByCustomerEMail != null)
          {
            throw new Exception("Sistemde zaten bu e-posta adresi ile kayıtlı bir müşteri bilgisi var.");
          }
        }


        if (customerExists != null)
        {
          customerExists.CustomerEMail = customer.CustomerEMail;
          customerExists.CustomerIsActive = customer.CustomerIsActive;
          customerExists.CustomerName = customer.CustomerName;
          customerExists.CustomerPhone = customer.CustomerPhone;
          customerExists.UpdatedDateTime = DateTime.Now;

          await _unitOfWork.CustomerRepository.Update(customerExists);
        }
        else
        {
          customer.CreatedDateTime = DateTime.Now;
          await _unitOfWork.CustomerRepository.Insert(customer);
        }

        response.Data = customer;
        var responseUnitOfWork = await _unitOfWork.Save();
        response.Status = responseUnitOfWork.Status;
        response.Message = responseUnitOfWork.Message;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }

      return response;
    }

    public async Task<OperationResponse<Entities.Model.Customer.Customer>> DeleteCustomerByCustomerId(int id)
    {
      OperationResponse<Entities.Model.Customer.Customer> response = new OperationResponse<Entities.Model.Customer.Customer>();

      try
      {
        var existsCustomer = await _unitOfWork.CustomerRepository.GetById(id);
        if (existsCustomer == null)
        {
          throw new Exception("Sistemde kayıtlı bir müşteri bilgisi bulunamadı.");
        }

        await _unitOfWork.CustomerRepository.Delete(existsCustomer);
        var responseUnitOfWork = await _unitOfWork.Save();
        response.Status = responseUnitOfWork.Status;
        response.Message = responseUnitOfWork.Message;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }
      return response;
    }

    public async Task<OperationResponse<Entities.Model.Customer.Customer>> GetByCustomerId(int id)
    {
      OperationResponse<Entities.Model.Customer.Customer> response = new OperationResponse<Entities.Model.Customer.Customer>();

      try
      {
        var existsCustomer = await _unitOfWork.CustomerRepository.GetById(id);
        response.Data = existsCustomer ?? throw new Exception("Sistemde kayıtlı bir müşteri bilgisi bulunamadı.");
        response.Status = true;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }
      return response;
    }

    public async Task<OperationResponse<Entities.Model.Customer.Customer>> GetCustomerByEmail(string customerEmail)
    {
      OperationResponse<Entities.Model.Customer.Customer> response = new OperationResponse<Entities.Model.Customer.Customer>();

      try
      {
        var existsCustomer = await _unitOfWork.CustomerRepository.GetCustomerByEmail(customerEmail);
        response.Data = existsCustomer ?? throw new Exception("Sistemde kayıtlı bir müşteri bilgisi bulunamadı.");
        response.Status = true;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }
      return response;
    }
  }
}
