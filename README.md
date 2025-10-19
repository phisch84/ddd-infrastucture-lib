# ddd-infrastucture-lib
Boiler plate code for the Infrastructure layer in a Domain Driven Design project. This library provides scaffolds with synchronous and asynchronous methods for:
* Abstraction of logging
* Data Access Objects (DAOs)
* Aspect Oriented Programming (AOP)
* Remoting

Goal of this library is to provide reusable and extendable code for common infrastructure tasks and to enable Domain Driven Design best practices. This is achieved by e.g. using Data Access Objects (DAOs) to abstract data access logic from the domain layer, allowing to switch between different data storage technologies without affecting the domain logic.
When applied correctly, simply changing the derived classes should be enough.
This way developers can start implementing with lean mocks that e.g. simply stored data in memory and later switch to more complex implementations that interact with real databases or remote services.

## Content
For the scaffolds mentioned above, the library provides the following classes and interfaces.

### Logging
Interfaces and classes for logging are located in the namespace `com.schoste.ddd.Infrastructure.V1.Logging`. The interfaces, classes and their concepts are used by other parts of the library (e.g., AOP aspects) and can also be used directly in the application code. This feature can be used in combination with Aspect Oriented Programming (AOP).
* `ILogger`: An interface defining methods for logging messages with different severity levels (Info, Warning, Error).
* `Log`: An abstract class providing a singleton instance to the concrete implementation of the `ILogger` interface. The actual logging implementation must be provided in a derived class and assigned to the `Instance` property by implementing the `Log.Initialize(ILogger logger)` method.

### Data Access Objects (DAOs)
Interfaces and classes for managing data access operations are located in the namespace `com.schoste.ddd.Infrastructure.V1.DAL`.
* `IDataAccessObject<T>`: An interface defining synchronous and asynchronous CRUD operations for a generic type `T`.
* `DataAccessObject<T>`: An abstract class implementing the management logic for the `IDataAccessObject<T>` interface.
The actual implementation of data access logic (e.g., database interactions) must be provided in derived classes.

### Aspect Oriented Programming (AOP)
Interfaces and classes for managing data access operations are located in the namespace `com.schoste.ddd.Infrastructure.V1.Aspects`. The APO feature is enabled by creating objects via the `com.schoste.ddd.Infrastructure.V1.Shared.Services.ObjectFactory` class and attributes which implement the `com.schoste.ddd.Infrastructure.V1.Aspects.IMethodAspect` interface.
* `IMethodAspect`: An interface defining methods for pre- and post-execution logic around method calls.
* `LoggedAspect`: An implementation for the `IMethodAspect` interface to provide logging functionality. The logging information are written to any implementation of the `com.schoste.ddd.Infrastructure.V1.Logging.ILogger` interface which is referenced by `com.schoste.ddd.Infrastructure.V1.Logging.Log.Instance`.
* `RemotedAspect`: An implementation for the `IMethodAspect` interface to enable remote method calls in a way, that makes the implementation seemingly similar to local method calls.
The remote calls are handled by the implementation of the `com.schoste.ddd.Infrastructure.V1.Remoting.IRemotingClient` interface for the client side (caller / recipient of the method's return values) and the `com.schoste.ddd.Infrastructure.V1.Remoting.IRemotingServer` interface for the server side (callee / executor of methods).

### Remoting
Interfaces and classes for managing remote method calls are located in the namespace `com.schoste.ddd.Infrastructure.V1.Remoting`. This feature uses the Aspect Oriented Programming (AOP).
* `IRemotingClient`: An interface defining methods for invoking remote method calls synchronously and asynchronously.
* `RemotingClient`: An abstract class implementing the management logic for the `IRemotingClient` interface. It handles the serialization of method parameters and and deserialization return values. The actual implementation of the communication logic (e.g., via HTTP, TCP, etc.) must be provided in derived classes.
* `IRemotingServer`: An interface defining methods for registering and handling remote method calls.
* `RemotingServer`: An abstract class implementing the management logic for the `IRemotingServer` interface. It handles the deserialization of incoming method calls and the serialization of return values. The actual implementation of the communication logic (e.g., via HTTP, TCP, etc.) must be provided in derived classes.