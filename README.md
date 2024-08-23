SELECT * FROM Novels WHERE 
Tags NOT LIKE '%yuri%' AND Tags NOT LIKE '%Yaoi%' AND Tags NOT LIKE '%Shoujo%' AND Tags NOT LIKE '%FemaleProtagonist%'
 AND Tags NOT LIKE '%urban%' 
  AND Tags NOT LIKE '%Fan-fiction%' 
  AND Tags NOT LIKE '%Gender-bender%' 
  AND Tags NOT LIKE '%BeautifulFemaleLead%' 
 AND CAST(Chapter as int) > 400
 ORDER BY Name;
