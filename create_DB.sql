-- calendar definition

CREATE TABLE `calendar` (
  `cal_id` tinyint NOT NULL AUTO_INCREMENT,
  `cal_remote_id` varchar(100) NOT NULL,
  `cal_bg_color` char(7) NOT NULL,
  `cal_border_color` char(7) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cal_text_color` char(7) DEFAULT NULL,
  PRIMARY KEY (`cal_id`)
);


-- company definition

CREATE TABLE `company` (
  `com_firstname` varchar(100) NOT NULL,
  `com_lastname` varchar(100) NOT NULL,
  `com_tradename` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `com_address` varchar(100) NOT NULL,
  `com_zip` varchar(100) DEFAULT NULL,
  `com_town` varchar(100) DEFAULT NULL,
  `com_phone` varchar(100) DEFAULT NULL,
  `com_duns` varchar(20) NOT NULL,
  `com_logo` longblob,
  PRIMARY KEY (`com_duns`)
) ;


-- customer definition

CREATE TABLE `customer` (
  `cus_id` int NOT NULL AUTO_INCREMENT,
  `cus_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cus_address` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cus_zip` varchar(6) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cus_town` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cus_phone` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cus_mobile` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cus_website` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cus_email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cus_created_on` date NOT NULL DEFAULT (curdate()),
  PRIMARY KEY (`cus_id`)
) ;


-- financial_movement definition

CREATE TABLE `financial_movement` (
  `fim_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `fim_amount` decimal(7,2) NOT NULL,
  `fim_date` date NOT NULL,
  `fim_third_party` varchar(100) DEFAULT NULL,
  `fim_description` varchar(100) DEFAULT NULL,
  `fim_vat` decimal(7,2) DEFAULT '0.00',
  PRIMARY KEY (`fim_id`)
) ;


-- payment_method definition

CREATE TABLE `payment_method` (
  `pay_id` tinyint NOT NULL AUTO_INCREMENT,
  `pay_name` varchar(100) NOT NULL,
  PRIMARY KEY (`pay_id`)
) ;


-- `role` definition

CREATE TABLE `role` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ;


-- service definition

CREATE TABLE `service` (
  `ser_id` mediumint NOT NULL AUTO_INCREMENT,
  `ser_designation` varchar(150) NOT NULL,
  `ser_price` decimal(6,2) NOT NULL,
  `ser_from` date NOT NULL,
  `ser_to` date DEFAULT NULL,
  PRIMARY KEY (`ser_id`)
) ;


-- tax_rate definition

CREATE TABLE `tax_rate` (
  `tar_id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `tar_start` date NOT NULL,
  `tar_end` date DEFAULT NULL,
  `tar_rate` decimal(4,4) NOT NULL,
  PRIMARY KEY (`tar_id`)
);


-- `user` definition

CREATE TABLE `user` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `username` varchar(20) NOT NULL,
  `password` varchar(100) NOT NULL,
  `firstname` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `lastname` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `picture` longblob,
  `email` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
);


-- invoice definition

CREATE TABLE `invoice` (
  `inv_id` int NOT NULL AUTO_INCREMENT,
  `inv_reference` varchar(10) DEFAULT NULL,
  `inv_purchase_order` varchar(20) DEFAULT NULL,
  `inv_amount` decimal(7,2) NOT NULL,
  `inv_taxe` bit(1) NOT NULL DEFAULT b'0',
  `pay_id` tinyint NOT NULL DEFAULT '1',
  `cus_id` int NOT NULL,
  `inv_emitted_on` date DEFAULT NULL,
  `inv_due_on` date DEFAULT NULL,
  `inv_paid_on` date DEFAULT NULL,
  PRIMARY KEY (`inv_id`),
  KEY `invoice_FK` (`pay_id`),
  KEY `invoice_FK_1` (`cus_id`),
  CONSTRAINT `invoice_FK` FOREIGN KEY (`pay_id`) REFERENCES `payment_method` (`pay_id`),
  CONSTRAINT `invoice_FK_1` FOREIGN KEY (`cus_id`) REFERENCES `customer` (`cus_id`)
);


-- invoice_detail definition

CREATE TABLE `invoice_detail` (
  `det_id` int NOT NULL AUTO_INCREMENT,
  `inv_id` int NOT NULL,
  `det_label` varchar(200) DEFAULT NULL,
  `det_quantity` decimal(5,1) NOT NULL,
  `det_discount` decimal(4,2) NOT NULL DEFAULT '0.00',
  `ser_id` mediumint NOT NULL,
  PRIMARY KEY (`det_id`),
  KEY `invoice_detail_FK` (`inv_id`),
  KEY `invoice_detail_FK_1` (`ser_id`),
  CONSTRAINT `invoice_detail_FK` FOREIGN KEY (`inv_id`) REFERENCES `invoice` (`inv_id`),
  CONSTRAINT `invoice_detail_FK_1` FOREIGN KEY (`ser_id`) REFERENCES `service` (`ser_id`)
);


-- user_role definition

CREATE TABLE `user_role` (
  `user_id` bigint NOT NULL,
  `role_id` int NOT NULL,
  PRIMARY KEY (`user_id`,`role_id`),
  KEY `user_roles_FK1` (`role_id`),
  CONSTRAINT `user_roles_FK1` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`),
  CONSTRAINT `user_roles_FK2` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);


-- sp_invoice_confidence definition
DELIMITER //

CREATE FUNCTION IF NOT EXISTS `sp_invoice_confidence`(dueOn DATE,paidOn DATE) RETURNS double
    DETERMINISTIC
BEGIN
	DECLARE diff INT;

	SET diff = DATEDIFF(ifnull(paidOn, curdate()), dueOn);

	IF (diff=0) THEN
		RETURN 1;
	ELSEIF (diff<0) THEN
		RETURN 1 + ifnull((log10(-diff) / 100), 0);
	ELSE
		RETURN GREATEST(1 - log10(2*diff),0);
	END IF;

END//

DELIMITER ;

-- extented_customer source

CREATE OR REPLACE VIEW `extented_customer` AS
select
    `c`.`cus_id` AS `cus_id`,
    `c`.`cus_name` AS `cus_name`,
    `c`.`cus_address` AS `cus_address`,
    `c`.`cus_zip` AS `cus_zip`,
    `c`.`cus_town` AS `cus_town`,
    `c`.`cus_phone` AS `cus_phone`,
    `c`.`cus_mobile` AS `cus_mobile`,
    `c`.`cus_website` AS `cus_website`,
    `c`.`cus_email` AS `cus_email`,
    `c`.`cus_created_on` AS `cus_created_on`,
    sum(`i`.`inv_amount`) AS `billed`,
    sum((case when (`i`.`inv_paid_on` is not null) then `i`.`inv_amount` else 0 end)) AS `paid`,
    greatest((1 / (3 * count(1))),
    least(1, avg(`sp_invoice_confidence`(`i`.`inv_due_on`, `i`.`inv_paid_on`)))) AS `confidence_index`
from
    `customer` `c`
left join `invoice` `i` on
    (`c`.`cus_id` = `i`.`cus_id`)
        and (`i`.`inv_emitted_on` is not null)
group by
    `c`.`cus_id`
order by
    `c`.`cus_id`;
