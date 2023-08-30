CREATE DATABASE IF NOT EXISTS `PIZZAS` DEFAULT CHARACTER SET UTF8MB4 COLLATE utf8_general_ci;
USE `PIZZAS`;

CREATE TABLE `COMMANDES` (
  `id_commande` int NOT NULL AUTO_INCREMENT,
  `type_commande` varchar(10) NOT NULL,
  `etat_commande` varchar(10) NOT NULL,
  `total_commande` DECIMAL(6,2) NOT NULL,
  `id_utilisateur` int NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`id_commande`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4;

CREATE TABLE `COMPOSER` (
  `id_commande` int NOT NULL AUTO_INCREMENT,
  `id_pizza` int NOT NULL AUTO_INCREMENT,
  `quantite` int NOT NULL,
  `prix` DECIMAL(6,2) NOT NULL,
  PRIMARY KEY (`id_commande`, `id_pizza`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4;

CREATE TABLE `PIZZA` (
  `id_pizza` int NOT NULL AUTO_INCREMENT,
  `nom_pizza` varchar(100) NOT NULL,
  `description_pizza` varchar(255) NOT NULL,
  `prix_pizza` DECIMAL(6,2) NOT NULL,
  `image_pizza` BLOB  NOT NULL,
  PRIMARY KEY (`id_pizza`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4;

CREATE TABLE `ROLES` (
  `id_role` int NOT NULL AUTO_INCREMENT,
  `nom_role` varchar(100) NOT NULL,
  PRIMARY KEY (`id_role`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4;

CREATE TABLE `UTILISATEURS` (
  `id_utilisateur` int NOT NULL AUTO_INCREMENT,
  `nom_utilisateur` varchar(100) NOT NULL,
  `prenom_utilisateur` varchar(100) NOT NULL,
  `adresse_utilisateur` varchar(MAX) NOT NULL,
  `telephone_utilisateur` nvarchar(12) NOT NULL,
  `motpasse_utilisateur` nvarchar(100) NOT NULL,
  `id_role` int NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`id_utilisateur`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4;

ALTER TABLE `COMMANDES` ADD FOREIGN KEY (`id_utilisateur`) REFERENCES `UTILISATEURS` (`id_utilisateur`);
ALTER TABLE `COMPOSER` ADD FOREIGN KEY (`id_pizza`) REFERENCES `PIZZA` (`id_pizza`);
ALTER TABLE `COMPOSER` ADD FOREIGN KEY (`id_commande`) REFERENCES `COMMANDES` (`id_commande`);
ALTER TABLE `UTILISATEURS` ADD FOREIGN KEY (`id_role`) REFERENCES `ROLES` (`id_role`);