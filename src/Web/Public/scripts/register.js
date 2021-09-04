{
    const register = document.getElementById("register");
    const button = document.getElementById("register-button");

    const inputs = [
        document.getElementById("register-name"),
        document.getElementById("register-email"),
        document.getElementById("register-password")
    ];

    const error = document.createElement("div");
    error.classList.add("register-error");

    register.onsubmit = async (event) => {
        event.preventDefault();

        error.remove();
        
        button.disabled = true;

        const body = {};

        for(let index = 0; index < inputs.length; index++) {
            inputs[index].classList.remove("error");

            inputs[index].disabled = true;

            body[inputs[index].name] = inputs[index].value;
        }

        const response = await fetch("/api/user/register", {
            method: "POST",
            
            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(body)
        });

        const data = await response.json();
        
        if(data.error) {
            for(let index = 0; index < inputs.length; index++)
                inputs[index].disabled = false;

            const field = inputs.find(x => x.name == data.field);
            
            field.classList.add("error");

            if(data.message) {
                error.innerText = data.message;

                field.parentElement.appendChild(error);
            }

            button.disabled = false;

            return false;
        }
        
        window.location.href = "/register/verification";

        return false;
    };
}
