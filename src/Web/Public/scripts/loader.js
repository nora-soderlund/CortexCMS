(async () => {
    const loader = document.getElementById("loader");
    
    const scripts = [
        "app/API.js",
        "app.js"
    ];

    for(let index = 0; index < scripts.length; index++) {
        await new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.type = "text/javascript";
            script.src = `/scripts/${scripts[index]}`;
        
            script.onload = () => resolve();
            script.onerror = () => reject();

            document.body.appendChild(script);
        });
    }
    
    loader.style.display = "none";
})();
