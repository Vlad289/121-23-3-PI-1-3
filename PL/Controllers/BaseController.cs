using BLL.DTOs;
using BLL.Interfaces;
using System.Reflection;
namespace PL.Controllers
{
    /// <summary>
    /// Base controller class that provides common functionality for all controllers
    /// </summary>
    public abstract class BaseController
    {
        protected readonly IServiceProvider _serviceProvider;
        public BaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Gets a service from the DI container
        /// </summary>
        protected T GetService<T>() where T : class
        {
            return _serviceProvider.GetService(typeof(T)) as T
                ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found in the container.");
        }
        /// <summary>
        /// Checks if an entity with the given ID exists
        /// </summary>
        protected async Task<bool> EntityExistsAsync<T>(int id) where T : class
        {
            // Find the appropriate service to check existence
            Type serviceType = GetServiceTypeForDto<T>();
            var service = _serviceProvider.GetService(serviceType);
            if (service == null)
                throw new InvalidOperationException($"Service for {typeof(T).Name} not found");
            // Find the existence check method
            MethodInfo? existsMethod = null;
            if (typeof(T) == typeof(ProductDto))
                existsMethod = serviceType.GetMethod("GetProductByIdAsync");
            else if (typeof(T) == typeof(UserDto))
                existsMethod = serviceType.GetMethod("GetUserByIdAsync");
            else if (typeof(T) == typeof(OrderDto))
                existsMethod = serviceType.GetMethod("GetOrderByIdAsync");
            else if (typeof(T) == typeof(OrderItemDto))
                existsMethod = serviceType.GetMethod("IsOrderItemExistsAsync");
            if (existsMethod == null)
                throw new InvalidOperationException($"Existence check method for {typeof(T).Name} not found");

            // For IsOrderItemExistsAsync, it returns bool directly
            if (existsMethod.Name == "IsOrderItemExistsAsync")
            {
                return await (Task<bool>)existsMethod.Invoke(service, new object[] { id })!;
            }

            // For other Get methods, they return the entity or null
            try
            {
                // Check the return type of the method
                Type returnType = existsMethod.ReturnType;

                if (returnType == typeof(Task))
                {
                    // Method returns Task (void)
                    await (Task)existsMethod.Invoke(service, new object[] { id })!;
                    return true; // If no exception was thrown, assume it exists
                }
                else
                {
                    // Method returns Task<T>
                    dynamic task = existsMethod.Invoke(service, new object[] { id })!;
                    var result = await task;
                    return result != null;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Gets the service type for a given DTO type
        /// </summary>
        private Type GetServiceTypeForDto<T>() where T : class
        {
            if (typeof(T) == typeof(ProductDto))
                return typeof(IProductService);
            else if (typeof(T) == typeof(UserDto))
                return typeof(IUserService);
            else if (typeof(T) == typeof(OrderDto))
                return typeof(IOrderService);
            else if (typeof(T) == typeof(OrderItemDto))
                return typeof(IOrderItemService);
            else
                throw new InvalidOperationException($"No service type mapping for {typeof(T).Name}");
        }
    }
}