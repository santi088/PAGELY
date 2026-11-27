using PAGELY.Models;

namespace PAGELY.Services;

/// <summary>
/// Sample catalog used until the online database is ready. Swap this class for the
/// real one in MauiProgram.cs and nothing else needs to change.
/// </summary>
public class MockCatalogService : IOnlineCatalogService
{
    private static readonly List<DiscoverBook> Books =
    [
        new() { Id = "b01", Title = "Pride and Prejudice", Author = "Jane Austen", Genre = "Romance", Year = 1813, Rating = 4.6, RatingCount = 1280,
            Description = "A quick-witted young woman and a proud, wealthy gentleman keep misjudging each other while family pressure and social rank get in the way." },
        new() { Id = "b02", Title = "Frankenstein", Author = "Mary Shelley", Genre = "Science Fiction", Year = 1818, Rating = 4.2, RatingCount = 940,
            Description = "A young scientist brings a creature to life and then runs from what he has made, with tragic results for everyone." },
        new() { Id = "b03", Title = "Dracula", Author = "Bram Stoker", Genre = "Horror", Year = 1897, Rating = 4.1, RatingCount = 870,
            Description = "Told through letters and diary entries, a group of friends races to stop an ancient count who is bringing his darkness to England." },
        new() { Id = "b04", Title = "The Adventures of Sherlock Holmes", Author = "Arthur Conan Doyle", Genre = "Mystery", Year = 1892, Rating = 4.5, RatingCount = 1120,
            Description = "A collection of short cases in which a brilliant detective and his loyal friend solve puzzles that baffle everyone else." },
        new() { Id = "b05", Title = "Alice's Adventures in Wonderland", Author = "Lewis Carroll", Genre = "Fantasy", Year = 1865, Rating = 4.3, RatingCount = 990,
            Description = "A girl follows a hurried rabbit down a hole and lands in a world where logic, size and manners all stop making sense." },
        new() { Id = "b06", Title = "The Picture of Dorian Gray", Author = "Oscar Wilde", Genre = "Classic", Year = 1890, Rating = 4.4, RatingCount = 760,
            Description = "A handsome young man stays untouched by time while a portrait of him slowly shows the cost of how he lives." },
        new() { Id = "b07", Title = "Moby-Dick", Author = "Herman Melville", Genre = "Adventure", Year = 1851, Rating = 3.9, RatingCount = 610,
            Description = "A sailor joins a whaling ship whose captain is driven by a single obsession: hunting one enormous white whale." },
        new() { Id = "b08", Title = "The Time Machine", Author = "H. G. Wells", Genre = "Science Fiction", Year = 1895, Rating = 4.0, RatingCount = 540,
            Description = "An inventor travels far into the future and finds a world that has changed in ways he never expected." },
        new() { Id = "b09", Title = "Treasure Island", Author = "Robert Louis Stevenson", Genre = "Adventure", Year = 1883, Rating = 4.2, RatingCount = 700,
            Description = "A boy finds a treasure map and sails out with a crew, only to learn that some of his shipmates are pirates." },
        new() { Id = "b10", Title = "Jane Eyre", Author = "Charlotte Brontë", Genre = "Romance", Year = 1847, Rating = 4.5, RatingCount = 830,
            Description = "An orphaned governess with a strong will takes a job at a mysterious house and has to decide what she is willing to give up for love." },
        new() { Id = "b11", Title = "The Hound of the Baskervilles", Author = "Arthur Conan Doyle", Genre = "Mystery", Year = 1902, Rating = 4.3, RatingCount = 650,
            Description = "A family curse and a ghostly hound on a foggy moor bring the famous detective to the countryside." },
        new() { Id = "b12", Title = "Little Women", Author = "Louisa May Alcott", Genre = "Classic", Year = 1868, Rating = 4.4, RatingCount = 910,
            Description = "Four sisters grow up together, chasing different dreams while their family faces hard times." },
        new() { Id = "b13", Title = "The Wonderful Wizard of Oz", Author = "L. Frank Baum", Genre = "Fantasy", Year = 1900, Rating = 4.1, RatingCount = 580,
            Description = "A farm girl is swept away to a colorful land and must find the wizard who might send her home." },
        new() { Id = "b14", Title = "Wuthering Heights", Author = "Emily Brontë", Genre = "Romance", Year = 1847, Rating = 4.0, RatingCount = 720,
            Description = "A wild, stormy tale of two families on the Yorkshire moors and a bond that turns into a lifelong grudge." },
        new() { Id = "b15", Title = "1984", Author = "George Orwell", Genre = "Science Fiction", Year = 1949, Rating = 4.5, RatingCount = 3100,
            Description = "A dystopian vision of a totalitarian future where Big Brother watches every move and thoughtcrime is punishable by death." },
        new() { Id = "b16", Title = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Classic", Year = 1960, Rating = 4.5, RatingCount = 2800,
            Description = "A child's-eye view of racial injustice in the Depression-era American South, as a lawyer defends a wrongly accused Black man." },
        new() { Id = "b17", Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Genre = "Classic", Year = 1925, Rating = 4.2, RatingCount = 2200,
            Description = "A tragic tale of wealth, love, and the American dream in the roaring twenties on Long Island's gold coast." },
        new() { Id = "b18", Title = "The Catcher in the Rye", Author = "J.D. Salinger", Genre = "Classic", Year = 1951, Rating = 3.9, RatingCount = 1700,
            Description = "A disaffected teenager wanders through New York City after being expelled from prep school, searching for authenticity." },
        new() { Id = "b19", Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", Year = 1937, Rating = 4.7, RatingCount = 3500,
            Description = "A homebody hobbit is swept into an adventure with dwarves and a wizard to reclaim a mountain from a dragon." },
        new() { Id = "b20", Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", Genre = "Fantasy", Year = 1954, Rating = 4.8, RatingCount = 4200,
            Description = "A humble hobbit must carry a powerful ring across Middle-earth to destroy it in the fires of Mount Doom." },
        new() { Id = "b21", Title = "The Chronicles of Narnia", Author = "C.S. Lewis", Genre = "Fantasy", Year = 1950, Rating = 4.4, RatingCount = 1900,
            Description = "Four children step through a wardrobe into a magical land trapped in eternal winter by a wicked witch." },
        new() { Id = "b22", Title = "Harry Potter and the Sorcerer's Stone", Author = "J.K. Rowling", Genre = "Fantasy", Year = 1997, Rating = 4.6, RatingCount = 4800,
            Description = "An orphan boy discovers he is a wizard and begins his education at Hogwarts School of Witchcraft and Wizardry." },
        new() { Id = "b23", Title = "The Hunger Games", Author = "Suzanne Collins", Genre = "Science Fiction", Year = 2008, Rating = 4.3, RatingCount = 3200,
            Description = "In a dystopian future, a teenage girl volunteers to fight in a televised battle to the death among districts." },
        new() { Id = "b24", Title = "Dune", Author = "Frank Herbert", Genre = "Science Fiction", Year = 1965, Rating = 4.5, RatingCount = 2700,
            Description = "A young nobleman's family is betrayed on a desert planet where the spice is the most valuable substance in the universe." },
        new() { Id = "b25", Title = "The Road", Author = "Cormac McCarthy", Genre = "Post-Apocalyptic", Year = 2006, Rating = 4.1, RatingCount = 900,
            Description = "A father and son walk a burned America, pushing a shopping cart with all their possessions toward the coast." },
        new() { Id = "b26", Title = "The Stand", Author = "Stephen King", Genre = "Horror", Year = 1978, Rating = 4.4, RatingCount = 1600,
            Description = "A superflu wipes out most of humanity, and survivors gather for a final showdown between good and evil." },
        new() { Id = "b27", Title = "It", Author = "Stephen King", Genre = "Horror", Year = 1986, Rating = 4.5, RatingCount = 2100,
            Description = "Seven childhood friends reunite as adults to face a shape-shifting evil that preys on their hometown of Derry, Maine." },
        new() { Id = "b28", Title = "The Shining", Author = "Stephen King", Genre = "Horror", Year = 1977, Rating = 4.3, RatingCount = 1400,
            Description = "A winter caretaker at an isolated hotel slowly descends into madness under the hotel's malevolent influence." },
        new() { Id = "b29", Title = "Pet Sematary", Author = "Stephen King", Genre = "Horror", Year = 1983, Rating = 4.1, RatingCount = 950,
            Description = "A family discovers a burial ground behind their home where the dead return wrong, leading to tragedy." },
        new() { Id = "b30", Title = "Misery", Author = "Stephen King", Genre = "Horror", Year = 1987, Rating = 4.3, RatingCount = 1100,
            Description = "A bestselling author is held captive by his biggest fan after a car crash leaves him injured and isolated." },
        new() { Id = "b31", Title = "The Girl with the Dragon Tattoo", Author = "Stieg Larsson", Genre = "Mystery", Year = 2005, Rating = 4.2, RatingCount = 1800,
            Description = "A journalist and a brilliant hacker investigate a decades-old disappearance in a powerful Swedish family." },
        new() { Id = "b32", Title = "Gone Girl", Author = "Gillian Flynn", Genre = "Mystery", Year = 2012, Rating = 4.1, RatingCount = 1600,
            Description = "A marriage unravels when a wife disappears on their fifth anniversary and her husband becomes the prime suspect." },
        new() { Id = "b33", Title = "The Silent Patient", Author = "Alex Michaelides", Genre = "Mystery", Year = 2019, Rating = 4.2, RatingCount = 1400,
            Description = "A woman shoots her husband and never speaks again, leaving a forensic therapist desperate to learn why." },
        new() { Id = "b34", Title = "The Da Vinci Code", Author = "Dan Brown", Genre = "Mystery", Year = 2003, Rating = 4.0, RatingCount = 2300,
            Description = "A symbologist and a cryptologist unravel a conspiracy hidden in the works of Leonardo da Vinci." },
        new() { Id = "b35", Title = "Angels & Demons", Author = "Dan Brown", Genre = "Mystery", Year = 2000, Rating = 4.1, RatingCount = 1700,
            Description = "A symbologist races to stop an ancient secret society from destroying Vatican City with antimatter." },
        new() { Id = "b36", Title = "The Name of the Rose", Author = "Umberto Eco", Genre = "Mystery", Year = 1980, Rating = 4.2, RatingCount = 900,
            Description = "A Franciscan friar investigates a series of murders in a 14th-century Italian monastery." },
        new() { Id = "b37", Title = "Rebecca", Author = "Daphne du Maurier", Genre = "Romance", Year = 1938, Rating = 4.3, RatingCount = 700,
            Description = "A young woman marries a wealthy widower and finds herself haunted by the memory of his first wife." },
        new() { Id = "b38", Title = "Jane Austen's Emma", Author = "Jane Austen", Genre = "Romance", Year = 1815, Rating = 4.2, RatingCount = 500,
            Description = "A well-meaning but meddlesome young woman plays matchmaker in a small English village with mixed results." },
        new() { Id = "b39", Title = "Sense and Sensibility", Author = "Jane Austen", Genre = "Romance", Year = 1811, Rating = 4.3, RatingCount = 600,
            Description = "Two sisters—one practical, one romantic—navigate love and heartbreak after their family loses its fortune." },
        new() { Id = "b40", Title = "North and South", Author = "Elizabeth Gaskell", Genre = "Classic", Year = 1855, Rating = 4.1, RatingCount = 400,
            Description = "A young woman moves from the rural south to an industrial northern town and falls for a mill owner." },
        new() { Id = "b41", Title = "Bleak House", Author = "Charles Dickens", Genre = "Classic", Year = 1853, Rating = 4.1, RatingCount = 500,
            Description = "A sprawling tale of a decades-long legal case that destroys the lives of everyone it touches." },
        new() { Id = "b42", Title = "Great Expectations", Author = "Charles Dickens", Genre = "Classic", Year = 1861, Rating = 4.3, RatingCount = 700,
            Description = "An orphan's life is transformed by a mysterious benefactor who sends him to London to become a gentleman." },
        new() { Id = "b43", Title = "A Tale of Two Cities", Author = "Charles Dickens", Genre = "Classic", Year = 1859, Rating = 4.2, RatingCount = 600,
            Description = "Against the backdrop of the French Revolution, two men's lives intertwine in sacrifice and redemption." },
        new() { Id = "b44", Title = "Oliver Twist", Author = "Charles Dickens", Genre = "Classic", Year = 1838, Rating = 4.1, RatingCount = 500,
            Description = "An orphan boy falls in with a gang of young thieves in Victorian London." },
        new() { Id = "b45", Title = "David Copperfield", Author = "Charles Dickens", Genre = "Classic", Year = 1850, Rating = 4.2, RatingCount = 450,
            Description = "A young man recounts his difficult childhood and rise to success in 19th-century England." },
        new() { Id = "b46", Title = "Les Misérables", Author = "Victor Hugo", Genre = "Classic", Year = 1862, Rating = 4.4, RatingCount = 700,
            Description = "An ex-convict seeks redemption while being pursued by a relentless inspector in post-revolutionary France." },
        new() { Id = "b47", Title = "The Hunchback of Notre-Dame", Author = "Victor Hugo", Genre = "Classic", Year = 1831, Rating = 4.0, RatingCount = 350,
            Description = "A deformed bell-ringer falls in love with a Romani dancer in medieval Paris." },
        new() { Id = "b48", Title = "War and Peace", Author = "Leo Tolstoy", Genre = "Classic", Year = 1869, Rating = 4.3, RatingCount = 800,
            Description = "Napoleon's invasion of Russia forms the backdrop for the lives of several aristocratic families." },
        new() { Id = "b49", Title = "Anna Karenina", Author = "Leo Tolstoy", Genre = "Classic", Year = 1877, Rating = 4.4, RatingCount = 700,
            Description = "A married aristocrat's affair with a young officer leads to social ruin and tragedy." },
        new() { Id = "b50", Title = "Crime and Punishment", Author = "Fyodor Dostoevsky", Genre = "Classic", Year = 1866, Rating = 4.4, RatingCount = 650,
            Description = "A destitute student murders a pawnbroker and is consumed by guilt and psychological torment." },
        new() { Id = "b51", Title = "The Brothers Karamazov", Author = "Fyodor Dostoevsky", Genre = "Classic", Year = 1880, Rating = 4.5, RatingCount = 500,
            Description = "Three brothers grapple with faith, doubt, and patricide in 19th-century Russia." },
        new() { Id = "b52", Title = "One Hundred Years of Solitude", Author = "Gabriel García Márquez", Genre = "Fantasy", Year = 1967, Rating = 4.3, RatingCount = 600,
            Description = "The Buendía family's story unfolds over a century in the magical town of Macondo." },
        new() { Id = "b53", Title = "Love in the Time of Cholera", Author = "Gabriel García Márquez", Genre = "Romance", Year = 1985, Rating = 4.1, RatingCount = 400,
            Description = "Two lovers are separated by circumstance but remain devoted to each other for over fifty years." },
        new() { Id = "b54", Title = "The Alchemist", Author = "Paulo Coelho", Genre = "Fantasy", Year = 1988, Rating = 4.2, RatingCount = 2500,
            Description = "A young shepherd travels from Spain to Egypt in search of a treasure and discovers his personal legend." },
        new() { Id = "b55", Title = "The Kite Runner", Author = "Khaled Hosseini", Genre = "Classic", Year = 2003, Rating = 4.5, RatingCount = 1300,
            Description = "A boy in Afghanistan betrays his friend and spends his life seeking redemption." },
        new() { Id = "b56", Title = "A Thousand Splendid Suns", Author = "Khaled Hosseini", Genre = "Classic", Year = 2007, Rating = 4.6, RatingCount = 1100,
            Description = "Two Afghan women form an unlikely bond as they endure decades of war and oppression." },
        new() { Id = "b57", Title = "Life of Pi", Author = "Yann Martel", Genre = "Adventure", Year = 2001, Rating = 4.1, RatingCount = 800,
            Description = "A boy survives a shipwreck and shares a lifeboat with a Bengal tiger across the Pacific Ocean." },
        new() { Id = "b58", Title = "The Book Thief", Author = "Markus Zusak", Genre = "Historical Fiction", Year = 2005, Rating = 4.5, RatingCount = 1200,
            Description = "Death narrates the story of a young German girl who steals books and shares them during World War II." },
        new() { Id = "b59", Title = "The Shadow of the Wind", Author = "Carlos Ruiz Zafón", Genre = "Mystery", Year = 2001, Rating = 4.4, RatingCount = 700,
            Description = "A boy discovers a mysterious book in a forgotten library and uncovers a dark secret in postwar Barcelona." },
        new() { Id = "b60", Title = "The Night Circus", Author = "Erin Morgenstern", Genre = "Fantasy", Year = 2011, Rating = 4.2, RatingCount = 600,
            Description = "Two young magicians compete in a mysterious circus that appears only at night." },
        new() { Id = "b61", Title = "Circe", Author = "Madeline Miller", Genre = "Fantasy", Year = 2018, Rating = 4.4, RatingCount = 700,
            Description = "The witch Circe is exiled to an island where she hones her craft and crosses paths with famous gods and heroes." },
        new() { Id = "b62", Title = "The Song of Achilles", Author = "Madeline Miller", Genre = "Fantasy", Year = 2011, Rating = 4.5, RatingCount = 600,
            Description = "A retelling of the Trojan War from the perspective of Patroclus, Achilles' closest companion." },
        new() { Id = "b63", Title = "Educated", Author = "Tara Westover", Genre = "Non-Fiction", Year = 2018, Rating = 4.5, RatingCount = 1200,
            Description = "A memoir of a woman who grew up in a survivalist family and earned a PhD despite never attending school." },
        new() { Id = "b64", Title = "Sapiens", Author = "Yuval Noah Harari", Genre = "Non-Fiction", Year = 2011, Rating = 4.3, RatingCount = 1500,
            Description = "A brief history of humankind, from the Stone Age to the present day." },
        new() { Id = "b65", Title = "Becoming", Author = "Michelle Obama", Genre = "Non-Fiction", Year = 2018, Rating = 4.6, RatingCount = 1100,
            Description = "The former First Lady's memoir of her life, from the South Side of Chicago to the White House." },
        new() { Id = "b66", Title = "Atomic Habits", Author = "James Clear", Genre = "Non-Fiction", Year = 2018, Rating = 4.5, RatingCount = 2000,
            Description = "A practical guide to building good habits and breaking bad ones through tiny changes." },
        new() { Id = "b67", Title = "The Subtle Art of Not Giving a F*ck", Author = "Mark Manson", Genre = "Non-Fiction", Year = 2016, Rating = 4.0, RatingCount = 1000,
            Description = "A counterintuitive approach to living a good life by focusing on what truly matters." },
        new() { Id = "b68", Title = "Thinking, Fast and Slow", Author = "Daniel Kahneman", Genre = "Non-Fiction", Year = 2011, Rating = 4.4, RatingCount = 900,
            Description = "A Nobel psychologist explains the two systems that drive the way we think and make decisions." },
        new() { Id = "b69", Title = "The Power of Habit", Author = "Charles Duhigg", Genre = "Non-Fiction", Year = 2012, Rating = 4.1, RatingCount = 600,
            Description = "An investigation into the science of why habits exist and how they can be changed." },
        new() { Id = "b70", Title = "Quiet", Author = "Susan Cain", Genre = "Non-Fiction", Year = 2012, Rating = 4.2, RatingCount = 500,
            Description = "A celebration of introverts and the overlooked power of quiet people in a world that can't stop talking." },
        new() { Id = "b71", Title = "Where the Crawdads Sing", Author = "Delia Owens", Genre = "Romance", Year = 2018, Rating = 4.5, RatingCount = 1800,
            Description = "A marsh girl in North Carolina becomes a murder suspect in this coming-of-age mystery." },
        new() { Id = "b72", Title = "The Seven Husbands of Evelyn Hugo", Author = "Taylor Jenkins Reid", Genre = "Romance", Year = 2017, Rating = 4.6, RatingCount = 1200,
            Description = "An aging Hollywood icon tells her life story to an unknown magazine reporter." },
        new() { Id = "b73", Title = "Daisy Jones & The Six", Author = "Taylor Jenkins Reid", Genre = "Romance", Year = 2019, Rating = 4.4, RatingCount = 900,
            Description = "A fictional rock band's rise and fall in the 1970s, told in an oral-history style." },
        new() { Id = "b74", Title = "Normal People", Author = "Sally Rooney", Genre = "Romance", Year = 2018, Rating = 4.0, RatingCount = 700,
            Description = "Two teenagers from a small Irish town navigate a complicated on-again-off-again relationship." },
        new() { Id = "b75", Title = "Conversations with Friends", Author = "Sally Rooney", Genre = "Romance", Year = 2017, Rating = 3.9, RatingCount = 400,
            Description = "Two college students become entangled with an older married couple in Dublin." },
        new() { Id = "b76", Title = "The Vanishing Half", Author = "Brit Bennett", Genre = "Historical Fiction", Year = 2020, Rating = 4.4, RatingCount = 800,
            Description = "Twin sisters run away from their Southern Black community and choose to live in two very different worlds." },
        new() { Id = "b77", Title = "The Water Dancer", Author = "Ta-Nehisi Coates", Genre = "Historical Fiction", Year = 2019, Rating = 4.1, RatingCount = 500,
            Description = "A young man born into slavery discovers a mysterious power that aids his escape on the Underground Railroad." },
        new() { Id = "b78", Title = "Homegoing", Author = "Yaa Gyasi", Genre = "Historical Fiction", Year = 2016, Rating = 4.4, RatingCount = 700,
            Description = "Two half-sisters in 18th-century Ghana lead parallel lives—one in slavery, one in freedom—and their descendants follow." },
        new() { Id = "b79", Title = "Americanah", Author = "Chimamanda Ngozi Adichie", Genre = "Romance", Year = 2013, Rating = 4.3, RatingCount = 600,
            Description = "A young Nigerian woman emigrates to America and navigates race, love, and identity." },
        new() { Id = "b80", Title = "Half of a Yellow Sun", Author = "Chimamanda Ngozi Adichie", Genre = "Historical Fiction", Year = 2006, Rating = 4.5, RatingCount = 500,
            Description = "Three lives intersect during the Nigerian Civil War and the struggle for Biafran independence." },
        new() { Id = "b81", Title = "Pachinko", Author = "Min Jin Lee", Genre = "Historical Fiction", Year = 2017, Rating = 4.5, RatingCount = 700,
            Description = "A Korean family's struggles and triumphs across four generations in Japan." },
        new() { Id = "b82", Title = "The Sympathizer", Author = "Viet Thanh Nguyen", Genre = "Historical Fiction", Year = 2015, Rating = 4.2, RatingCount = 400,
            Description = "A double agent navigates the aftermath of the Vietnam War as a spy among South Vietnamese refugees." },
        new() { Id = "b83", Title = "A Little Life", Author = "Hanya Yanagihara", Genre = "Classic", Year = 2015, Rating = 4.5, RatingCount = 900,
            Description = "Four college friends move to New York, but one carries a dark secret that shapes all their lives." },
        new() { Id = "b84", Title = "The Goldfinch", Author = "Donna Tartt", Genre = "Classic", Year = 2013, Rating = 4.1, RatingCount = 800,
            Description = "A boy survives a terrorist attack at a museum and walks away with a priceless painting." },
        new() { Id = "b85", Title = "The Secret History", Author = "Donna Tartt", Genre = "Mystery", Year = 1992, Rating = 4.3, RatingCount = 600,
            Description = "A group of classics students at a Vermont college commit a murder and try to cover it up." },
        new() { Id = "b86", Title = "The Handmaid's Tale", Author = "Margaret Atwood", Genre = "Science Fiction", Year = 1985, Rating = 4.3, RatingCount = 1400,
            Description = "In a totalitarian theocracy, a woman is forced to serve as a surrogate for the ruling class." },
        new() { Id = "b87", Title = "Oryx and Crake", Author = "Margaret Atwood", Genre = "Science Fiction", Year = 2003, Rating = 4.1, RatingCount = 500,
            Description = "After a genetic catastrophe, a man called Snowman may be the last human on Earth." },
        new() { Id = "b88", Title = "Never Let Me Go", Author = "Kazuo Ishiguro", Genre = "Science Fiction", Year = 2005, Rating = 4.2, RatingCount = 500,
            Description = "Students at an English boarding school slowly learn the true purpose of their existence." },
        new() { Id = "b89", Title = "The Remains of the Day", Author = "Kazuo Ishiguro", Genre = "Classic", Year = 1989, Rating = 4.3, RatingCount = 400,
            Description = "An aging butler reflects on his life of service and missed opportunities during a road trip." },
        new() { Id = "b90", Title = "Brave New World", Author = "Aldous Huxley", Genre = "Science Fiction", Year = 1932, Rating = 4.3, RatingCount = 1300,
            Description = "In a genetically engineered utopia, one man questions the cost of perfect happiness." },
        new() { Id = "b91", Title = "Fahrenheit 451", Author = "Ray Bradbury", Genre = "Science Fiction", Year = 1953, Rating = 4.3, RatingCount = 1200,
            Description = "A fireman who burns books begins to question his society's ban on reading." },
        new() { Id = "b92", Title = "The Martian", Author = "Andy Weir", Genre = "Science Fiction", Year = 2011, Rating = 4.5, RatingCount = 1500,
            Description = "An astronaut stranded on Mars must use science and ingenuity to survive until rescue." },
        new() { Id = "b93", Title = "Project Hail Mary", Author = "Andy Weir", Genre = "Science Fiction", Year = 2021, Rating = 4.7, RatingCount = 1100,
            Description = "A lone astronaut wakes up with amnesia and must save Earth from an interstellar threat." },
        new() { Id = "b94", Title = "Ender's Game", Author = "Orson Scott Card", Genre = "Science Fiction", Year = 1985, Rating = 4.4, RatingCount = 1000,
            Description = "A brilliant child is trained at a space military academy to command Earth's defenses against aliens." },
        new() { Id = "b95", Title = "Dune Messiah", Author = "Frank Herbert", Genre = "Science Fiction", Year = 1969, Rating = 4.0, RatingCount = 400,
            Description = "The sequel to Dune follows Paul Atreides as emperor facing rebellion and prophecy." },
        new() { Id = "b96", Title = "Children of Dune", Author = "Frank Herbert", Genre = "Science Fiction", Year = 1976, Rating = 4.1, RatingCount = 350,
            Description = "Paul's children inherit his mantle as the desert planet Arrakis faces new threats." },
        new() { Id = "b97", Title = "Foundation", Author = "Isaac Asimov", Genre = "Science Fiction", Year = 1951, Rating = 4.2, RatingCount = 700,
            Description = "A mathematician predicts the fall of galactic civilization and founds a preserve of knowledge." },
        new() { Id = "b98", Title = "I, Robot", Author = "Isaac Asimov", Genre = "Science Fiction", Year = 1950, Rating = 4.1, RatingCount = 600,
            Description = "Interconnected stories explore the three laws of robotics and humanity's future." },
        new() { Id = "b99", Title = "The Left Hand of Darkness", Author = "Ursula K. Le Guin", Genre = "Science Fiction", Year = 1969, Rating = 4.3, RatingCount = 500,
            Description = "An envoy visits a planet where inhabitants have no fixed gender." },
        new() { Id = "b100", Title = "The Dispossessed", Author = "Ursula K. Le Guin", Genre = "Science Fiction", Year = 1974, Rating = 4.4, RatingCount = 400,
            Description = "A physicist travels between two twin planets with opposing economic systems." },
    ];

    public async Task<IReadOnlyList<DiscoverBook>> GetBooksAsync(
        string? genre,
        DiscoverSort sort,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        IEnumerable<DiscoverBook> query = Books;

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => string.Equals(b.Genre, genre, StringComparison.OrdinalIgnoreCase));
        }

        query = sort switch
        {
            DiscoverSort.Newest => query.OrderByDescending(b => b.Year).ThenBy(b => b.Title),
            DiscoverSort.TitleAZ => query.OrderBy(b => b.Title),
            _ => query.OrderByDescending(b => b.Rating).ThenByDescending(b => b.RatingCount)
        };

        return query.ToList();
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);

        return Books
            .Select(b => b.Genre)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();
    }

    public Task<DiscoverBook?> GetBookAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Books.FirstOrDefault(b => b.Id == id));
    }

    public async Task<IReadOnlyList<DiscoverBook>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var q = query.Trim();
        return Books
            .Where(b =>
                b.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.Rating)
            .ThenBy(b => b.Title)
            .ToList();
    }

    public Task<IReadOnlyList<DiscoverBook>> GetDiscoveryAsync(string shelf, CancellationToken cancellationToken = default)
    {
        var sorted = shelf.ToLowerInvariant() switch
        {
            "recent" => Books.OrderByDescending(b => b.Year).ThenBy(b => b.Title),
            "top-rated" => Books.Where(b => b.RatingCount > 0).OrderByDescending(b => b.Rating).ThenByDescending(b => b.RatingCount),
            _ => Books.OrderByDescending(b => b.RatingCount).ThenByDescending(b => b.Rating)
        };

        return Task.FromResult<IReadOnlyList<DiscoverBook>>(sorted.Take(12).ToList());
    }
}
