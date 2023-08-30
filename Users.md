**ORDER** (<ins>order_id</ins>, date, total_amount, _#user_id_)<br>
**ORDER_LINE** (<ins>order_line_id</ins>, quantity, _#order_id_, _#pizza_id_)<br>
**PIZZA** (<ins>pizza_id</ins>, name, description, image, price)<br>
**ROLES** (<ins>role_id</ins>, name)<br>
**USERS** (<ins>user_id</ins>, phonenumber, password, firstname, lastname, address)<br>
**USER_ROLES** (<ins>_#user_id_</ins>, <ins>_#role_id_</ins>)