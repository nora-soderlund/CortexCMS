{
    const login = document.getElementById("login");
    const name = document.getElementById("login-name");
    const password = document.getElementById("login-password");
    const button = document.getElementById("login-button");

    login.onsubmit = async (event) => {
        event.preventDefault();
        
        name.disabled = true;
        password.disabled = true;
        button.disabled = true;

        const response = await fetch("/api/user/authorize", {
            method: "POST",
            
            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify({
                name: name.value,
                password: password.value
            })
        });

        const data = await response.json();

        console.log(data);

        return false;
    };
}
