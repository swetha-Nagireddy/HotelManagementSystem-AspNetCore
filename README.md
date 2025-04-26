# Hotel Management System - ASP.NET Core MVC

A Hotel Management System built using ASP.NET Core MVC to manage hotel room bookings, user registration, and login functionality and user Profile Updates. This project demonstrates the use of ASP.NET Core, Entity Framework, Dappers, Auto Mappers, JWT Authentication
Routing and Dependency Injection.

## Features

- **User Registration & Login** : Users can register an account and log in with JWT-based authentication.
- **Room Booking**              : Users can browse available rooms, book them, and view booking history.
- **Profile Management**        : Users can update their profile information.
- **Room Availability**         : Admins can manage room availability and pricing.
- **Pagination**                : Room listings are paginated for better UI.
- **Search Functionality**      : Rooms can be searched by type or price.
- **Responsive Design**         : Fully responsive design using Bootstrap.

## Technologies Used

- **ASP.NET Core MVC**
- **Entity Framework Core (Code First Approach)** - for User Registration, Login, User Feedback
- **JWT Authentication**  - for User Login
- **SQL Server (Database)** - For storing the user Data
- **Bootstrap** -  for Better UI
- **Dapper**  - for Room Booking , Geting Booking History, Getting Available Room, Adding the Room, Editing the Room Details 
- **Auto MApper** - To save the user Feedback in DataBase 
## Installation

### Prerequisites

1. [Visual Studio 2022 ](https://visualstudio.microsoft.com/downloads/)
2. .NET Core SDK 3.1 or later
3. SQL Server ( a remote instance)


**How It Works**
----------------------
User Registration & Login :

Users can register and log in to the system using the provided forms. On successful login, they receive a JWT token that is used for subsequent requests requiring authentication.
After SuccesFul Registration User redirects to their dashboard where they have different management options like USER PROFILE MANAGEMENT, BOOKING MANAGEMENT , AVAILABLE ROOMS  MANAGEMENT AND FEEDBACK SUBMISSION. 

**DESCRIPTION OF THE MANAGEMENT OPTIONS**

USER PROFILE MANAGEMENT :
The users can view thie profile and also able to update the data if they want. 

BOOKING MANAGEMENT :
In Booking Management the user can able to book the room. and also able to see the the booking history of the user.

Room Management :
In Room Management the user can able to see all the room available and also have option for searching the type of room they want and they can search by the RoomType and Price according to their preference they get the data.

FeedBack Submission :
And the user can also share their experience that they feel about the Hotel and also can share their experience and also can give the rating for the service Management.

Search & Pagination
Users can search for rooms by type or price, and the results are displayed with pagination (5 rooms per page).



And in this project i also implemented the data validation where it checks some parameters in the register and login form if user give incorrect data and these validations helps to identify the error. And i also implemented the ecetion handling concept to track the exceptions 
if there are any error occurs without crashing the application we terminate that exception in safe mode. And I also implemented the Serilog Loggers to log what are the things happening in the application.

