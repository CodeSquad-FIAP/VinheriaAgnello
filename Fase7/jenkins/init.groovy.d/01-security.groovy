// Usuário administrador fixo (o wizard de instalação está desligado na imagem).
import jenkins.model.*
import hudson.security.*

def instance = Jenkins.get()
def realm = new HudsonPrivateSecurityRealm(false)
realm.createAccount('admin', 'admin')
instance.setSecurityRealm(realm)
instance.setAuthorizationStrategy(new FullControlOnceLoggedInAuthorizationStrategy())
instance.save()
println '[init] usuario admin criado'
