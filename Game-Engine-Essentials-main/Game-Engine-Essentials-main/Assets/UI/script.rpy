# The script of the game goes in this file.

# Declare characters used by this game. The color argument colorizes the
# name of the character.

define e = Character("Eileen", color = "#7b2cbf")
define e_nvl = Character("Eileen", kind=nvl, color="#7b2cbf")


# The game starts here.

label start:

    # Show a background. This uses a placeholder by default, but you can
    # add a file (named either "bg room.png" or "bg room.jpg") to the
    # images directory to show it.

    scene bg room

    # This shows a character sprite. A placeholder is used, but you can
    # replace it by adding a file named "eileen happy.png" to the images
    # directory.

    # show eileen happy
    show Eileen

    # These display lines of dialogue.

    e "This is Halloween GUI kit for Ren'Py or other visual novels"
    
    e "Purchase it to use it on your commercial and non-commercial projects"

    e "Edit it however you like"

    e "Credit is not needed but is appreciated"

    menu features:
        "Try out this features"
        "Nvl Mode":
            e_nvl "This is the nvl mode"
            e_nvl "I like bosa nova, shoe gaze, and dream pop music"
            e_nvl "Uchu nekoko song's are so addictive"
            e_nvl "Check them out"
        "Notify":
            $ renpy.notify("Thank you!")
            e "This is the notify"
            $ renpy.notify("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAh")
            e "anything else"
            $ renpy.notify("Good luck")
            jump features
        "I'm okay":
            e "Okay bye bye"
            
    e "Please check out my other works at {a=https://chrisux.itch.io/}{color=#1da1f2}https://chrisux.itch.io/{/color}{/a}"
    
    e "For concerns or inquiries email me at mstri0913@gmail.com or message me through discord @nobb0209"
    
    e "Thank you"
    
    e "I hope you're next game will be great"
    
    e "I believe in you, Good luck"
    
    e "Message me your game that uses this theme I'll be happy to check it out"

    # This ends the game.

    return
