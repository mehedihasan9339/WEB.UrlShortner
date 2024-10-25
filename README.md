# URL Shortener Application

## Overview
The URL Shortener Application is a web-based tool that converts long URLs into short, easily shareable links. It supports custom aliases, click tracking, QR code generation, and email notifications to enhance user experience and convenience.

---

## How the Application Works

1. **URL Shortening**:  
   Users input a long URL, with the option to specify a custom alias. If left blank, a random alias is generated automatically.

2. **Custom Aliases**:  
   Users can create custom aliases for their URLs with a real-time availability check.

3. **Click Tracking**:  
   The application tracks each click on a shortened URL, displaying click counts for logged-in users.

4. **QR Code Generation**:  
   Each shortened URL is accompanied by a QR code, allowing easy access from mobile devices.

5. **Email Notifications**:  
   Upon creating a new short URL, the application sends an email notification with short URL details to the user.

6. **User Authentication**:  
   Advanced features, including custom alias creation, click tracking, and email notifications, require users to log in. This ensures secure URL management.

---

## Technologies and Tools Used

- **ASP.NET Core MVC**: Routing, controllers, and views.
- **Entity Framework Core**: Data management and CRUD operations.
- **SQL Server**: Stores URLs, aliases, click counts, and user data.
- **ASP.NET Identity**: Manages user authentication and authorization.
- **jQuery & AJAX**: Real-time alias availability checks.
- **Bootstrap**: Provides a responsive UI.
- **QR Code Generation Library**: Generates QR codes for URLs.
- **SMTP Email Service**: Sends email notifications with URL details.

---

## Screenshot


![Application Screenshot](https://raw.githubusercontent.com/mehedihasan9339/WEB.UrlShortner/refs/heads/master/WEB.UrlShortner/url-shortner-table.png)


![Application Screenshot](https://raw.githubusercontent.com/mehedihasan9339/WEB.UrlShortner/refs/heads/master/WEB.UrlShortner/url-shortner-chrats.png)


---

Developed by  
[Mehedi Hasan](mailto:mehedihasan9339@gmail.com)
