// Job do pipeline apontando para o Jenkinsfile versionado no repositório.
import jenkins.model.*
import org.jenkinsci.plugins.workflow.job.WorkflowJob
import org.jenkinsci.plugins.workflow.cps.CpsScmFlowDefinition
import hudson.plugins.git.*

def instance = Jenkins.get()
def nome = 'vinheria-pipeline'

if (instance.getItem(nome) == null) {
    def job = instance.createProject(WorkflowJob, nome)
    job.description = 'Build e deploy dos microsserviços da Vinheria Agnello (Fase 7 — DevOps)'
    def scm = new GitSCM(
        [new UserRemoteConfig('https://github.com/CodeSquad-FIAP/VinheriaAgnello.git', null, null, null)],
        [new BranchSpec('*/main')],
        false, [], null, null, [])
    job.definition = new CpsScmFlowDefinition(scm, 'Fase7/jenkins/Jenkinsfile')
    job.save()
    println "[init] job ${nome} criado"
} else {
    println "[init] job ${nome} ja existe"
}
