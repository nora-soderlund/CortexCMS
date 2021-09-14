{
    const login = document.getElementById("login");
    const button = document.getElementById("login-button");

    const inputs = [
        document.getElementById("login-name"),
        document.getElementById("login-password")
    ];

    login.onsubmit = async (event) => {
        event.preventDefault();
        
        button.disabled = true;

        const body = {};

        for(let index = 0; index < inputs.length; index++) {
            inputs[index].classList.remove("error");

            inputs[index].disabled = true;

            body[inputs[index].name] = inputs[index].value;
        }

        const response = await fetch("/api/user", {
            method: "POST",
            
            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(body)
        });

        const data = await response.json();

        console.log(data);
        
        if(data.error) {
            for(let index = 0; index < inputs.length; index++) {
                inputs[index].disabled = false;

                inputs[index].classList.add("error");
            }

            button.disabled = false;

            return false;
        }

        window.location.href = "/";

        return false;
    };
}
