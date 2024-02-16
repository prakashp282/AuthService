using System;

namespace Auth.AuthenticationService.Exceptions;

//We Introduce custom exceptions for central exception handling.
public class UserAlreadyExists(string message) : Exception(message);

public class UserDoesNotExistException(string message) : Exception(message);

public class UserCreationFailedException(string message) : Exception(message);

public class UserVerificationException(string message) : Exception(message);

public class UserUpdateFailedException(string message) : Exception(message);

public class PasswordChangeException(string message) : Exception(message);

public class UserLockedOutException(string message) : Exception(message);

public class RoleDoesNotExistException(string message) : Exception(message);

public class AddRoleException(string message) : Exception(message);

public class EmailMisMatchException(string message) : Exception(message);

public class DuplicatePasswordException(string message) : Exception(message);

public class PhoneAlreadyExists(string message) : Exception(message);

public class InvalidScopeException(string message) : Exception(message);

public class InvalidGrantException(string message) : Exception(message);

public class AccessDeniedException(string message) : Exception(message);