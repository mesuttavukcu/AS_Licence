﻿using AS_Licence.Data.Interface.UnitOfWork;
using AS_Licence.Entites.Validation.Entity.User;
using AS_Licence.Entities.ViewModel.Operations;
using AS_Licence.Helpers.Extension;
using AS_Licence.Service.Interface.User;
using System;
using System.Threading.Tasks;

namespace AS_Licence.Service.Host.User
{
  public class UserService : IUserManager
  {
    public readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public async Task<OperationResponse<Entities.Model.User.User>> RegisterUser(Entities.Model.User.User user, string password)
    {
      OperationResponse<Entities.Model.User.User> response = new OperationResponse<Entities.Model.User.User>();

      try
      {
        var valid = await new UserValidator().ValidateAsync(user);
        if (valid.IsValid == false)
        {
          throw new Exception(valid.GetErrorMessagesOnSingleLine());
        }

        response.Data = await _unitOfWork.AuthRepository.Register(user, password);

        var resultUnitOfWork = await _unitOfWork.Save();
        response.Status = resultUnitOfWork.Status;
        response.Message = resultUnitOfWork.Message;
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }

      return response;
    }

    public async Task<OperationResponse<Entities.Model.User.User>> LoginUser(string username, string password)
    {
      OperationResponse<Entities.Model.User.User> response = new OperationResponse<Entities.Model.User.User>();

      try
      {
        response.Data = await _unitOfWork.AuthRepository.Login(username.ToLower(), password);
        response.Status = response.Data != null;

        if (response.Status == false)
        {
          response.Message = "Kullanıcı adı ve şifreniz yanlış veya aktif kullanıcı değil!";
        }
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }

      return response;
    }

    public async Task<OperationResponse<bool>> UserExists(string username)
    {
      OperationResponse<bool> response = new OperationResponse<bool>();

      try
      {
        response.Status = await _unitOfWork.AuthRepository.UserExists(username.ToLower());
        if (response.Status)
        {
          response.Message = "Böyle bir kullanıcı zaten mevcut";
        }
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }

      return response;
    }

    public async Task<OperationResponse<Entities.Model.User.User>> GetUserInformationByUsername(string username)
    {
      OperationResponse<Entities.Model.User.User> response = new OperationResponse<Entities.Model.User.User>();

      try
      {
        response.Data = await _unitOfWork.AuthRepository.GetUserInformationByUsername(username.ToLower());
        if (response.Data == null)
        {
          response.Message = "Böyle bir kullanıcı yok.";
        }
      }
      catch (Exception e)
      {
        response.Status = false;
        response.Message = e.Message;
      }

      return response;
    }

    public async Task<OperationResponse<bool>> ChangeUserPassword(string username, string password, string newPassword, string newAgainPassword)
    {
      OperationResponse<bool> response = new OperationResponse<bool>();

      try
      {
        var existsUserWithPassword = await this.LoginUser(username, password);
        if (existsUserWithPassword.Status == false)
        {
          throw new Exception(existsUserWithPassword.Message);
        }

        if (newPassword.Length <= 5 || newAgainPassword.Length <= 5)
        {
          throw new Exception("Yeni şifreniz 5 karakterden büyük olmalıdır.");
        }

        if (newPassword != newAgainPassword)
        {
          throw new Exception("Yeni şifreniz ve tekrarı uyuşmuyor.");
        }

        await _unitOfWork.AuthRepository.ChangeUserPassword(username.ToLower(), newPassword);
        var resultUnitOfWork = await _unitOfWork.Save();
        response.Status = resultUnitOfWork.Status;
        response.Message = response.Status ? "Şifre değiştirme işlemi başarılı." : resultUnitOfWork.Message;
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
