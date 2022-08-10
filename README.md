This is a windows service which takes all orders from a certain client, creates a token with all data from this client as username and password and serializes a json from each order with all collected data. 
Then the application sends all orders for the shipping company api.
The api will return the response.
If the status code from the response is ok, the service will take the response content and access a directory file which has a default zpl code to make a new tag for each order.
Once the application got the default zpl code, it'll replace all zpl parameters with the returned fields from the sent order and make the new order tag.
Then the new tag and the order id will be inserted at the database.
The application runs every 10 minutes.
